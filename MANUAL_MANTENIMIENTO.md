# Manual de mantenimiento — StyleForge API

Este documento es para quien va a **programar** sobre este proyecto (agregar features, corregir bugs, hacer code review). No repite lo que ya está en el [README](README.md) (cómo levantar el proyecto, endpoints, formato de errores) — se enfoca en cómo está armado por dentro y qué hay que saber antes de tocar código.

---

## 1. Arquitectura en capas

```
StyleForge.Domain          → Entidades puras (sin dependencias de EF ni de nada externo)
        ↑
StyleForge.Application     → DTOs + interfaces de servicios (contratos, sin implementación)
        ↑
StyleForge.Infrastructure  → EF Core, implementaciones de los servicios, JWT, migraciones
        ↑
StyleForge.API             → Controllers, middleware, Program.cs (arma todo con DI)
```

La flecha indica "quién depende de quién". `Domain` no sabe que existe `Application`. `Infrastructure` implementa las interfaces que define `Application`. `API` inyecta esas implementaciones vía `Program.cs`.

`StyleForge.Tests` referencia `Infrastructure` directamente y usa un `AppDbContext` con proveedor **InMemory** de EF Core (ver [`DbContextHelper.cs`](StyleForge.Tests/Helpers/DbContextHelper.cs)) — no hay tests contra Postgres real.

---

## 2. Multi-tenancy — cómo funciona de verdad

Cada salón registrado es un `Tenant`. El aislamiento **no** es a nivel de base de datos (no hay schemas ni bases separadas) — es un **global query filter de EF Core** en [`AppDbContext.cs`](StyleForge.Infrastructure/Data/AppDbContext.cs):

```csharp
modelBuilder.Entity<Client>()
    .HasQueryFilter(x => _currentUser.TenantId == null || x.TenantId == _currentUser.TenantId);
```

Esto aplica a `User`, `Client`, `Service`, `Appointment`. **`Tenant` mismo no tiene filtro** (obviamente — necesitas poder leer tu propio tenant sin filtrar por tenant).

El flujo es:

1. Al hacer login, [`JwtService.cs`](StyleForge.Infrastructure/Services/JwtService.cs) mete el claim `tenantId` en el JWT.
2. [`CurrentUserService.cs`](StyleForge.API/Services/CurrentUserService.cs) lee ese claim del `HttpContext` en cada request (`_httpContextAccessor.HttpContext?.User.FindFirst("tenantId")`).
3. `AppDbContext` inyecta `ICurrentUserService` y lo usa en el query filter de arriba — por eso **cualquier query a esas 4 tablas queda automáticamente filtrada por tenant sin que el código del servicio tenga que acordarse de hacerlo**.

**Importante:** esto significa que el aislamiento depende 100% de que el claim `tenantId` del JWT sea correcto. Si algún día agregan un endpoint que use `[AllowAnonymous]` o que construya un `AppDbContext` fuera del pipeline HTTP normal (un job en background, por ejemplo), el filtro no se aplica solo — hay que pasarlo explícito.

**Hueco de seguridad conocido:** `TenantsController.Update` ([TenantsController.cs:23-28](StyleForge.API/Controllers/TenantsController.cs:23)) recibe el `id` del tenant a actualizar **desde la URL**, sin `[Authorize(Roles="Admin")]` ni verificar que ese `id` sea el tenant del usuario autenticado. Y como `Tenant` no tiene query filter, **cualquier usuario autenticado (Admin, empleado o incluso Client) de cualquier salón puede modificar los datos de cualquier otro salón** si conoce o adivina su GUID. Esto hay que arreglarlo: o se ignora el `id` de la ruta y se usa siempre `_currentUser.TenantId`, o se valida que coincidan antes de llamar al servicio.

---

## 3. Autenticación y JWT

- Passwords con **BCrypt** (`BCrypt.Net.BCrypt.HashPassword` / `.Verify`) — ver [`AuthService.cs`](StyleForge.Infrastructure/Services/AuthService.cs).
- El JWT lleva 4 claims: `tenantId` (custom), `NameIdentifier` (id del user/client), `Email`, `Role`.
- Hay dos formas de generar token: `GenerateToken(User)` para Admin/empleado y `GenerateClientToken(Client)` para clientes — porque `Client` y `User` son entidades distintas, no hay una tabla unificada de "cuentas".
- Los controllers de `AuthService` (Login, LoginClient) lanzan `Exception("Invalid credentials")` genérica y el controller la atrapa localmente con try/catch → `Unauthorized`. Esto es distinto al patrón de otros servicios (ver sección 6) — aquí no pasa por el exception handler global.

---

## 4. Cómo agregar un módulo nuevo

El patrón que sigue todo el proyecto (Clients, Employees, Services, Appointments):

1. **Entidad** en `StyleForge.Domain/Entities/` — heredar de `BaseEntity` si necesita `Id` + `TenantId` (casi siempre sí).
2. **DTOs + interfaz** en `StyleForge.Application/DTOs/<Módulo>/` y `StyleForge.Application/Interfaces/I<Módulo>Service.cs`.
3. **Implementación** en `StyleForge.Infrastructure/Services/<Módulo>Service.cs`, inyectando `AppDbContext` y `ICurrentUserService` si necesita el tenant actual.
4. **Registrar en DI** — agregar `builder.Services.AddScoped<I<Módulo>Service, <Módulo>Service>();` en [`Program.cs`](StyleForge.API/Program.cs).
5. **Controller** en `StyleForge.API/Controllers/<Módulo>Controller.cs` con `[Authorize]` (y `[Authorize(Roles = "Admin")]` en las acciones que lo requieran).
6. Si la entidad nueva necesita aislamiento por tenant, **agregar el `HasQueryFilter`** en `AppDbContext.OnModelCreating` — no es automático por heredar `BaseEntity`, hay que declararlo a mano por cada `DbSet`.
7. Generar la migración (ver sección 5).

**Convención de excepciones → status code**, mapeada en el exception handler global de [`Program.cs`](StyleForge.API/Program.cs:108):

| Excepción | Status | Uso |
|---|---|---|
| `ArgumentException` | 400 | Dato de entrada inválido |
| `KeyNotFoundException` | 404 | Recurso no encontrado |
| `InvalidOperationException` | 409 | Conflicto de regla de negocio (ej. solapamiento de horario) |
| cualquier otra | 500 | Se loguea con `app.Logger.LogError` y al cliente se le devuelve un mensaje genérico, **no** el mensaje real de la excepción |

Si vas a lanzar una excepción nueva en un servicio, usa una de estas — no `Exception` genérica, porque cae en 500 y se pierde el detalle para el cliente.

---

## 5. Migraciones EF Core

```bash
# Desde StyleForge.Infrastructure
dotnet ef migrations add NombreDeLaMigracion --startup-project ..\StyleForge.API
dotnet ef database update --startup-project ..\StyleForge.API
```

**Gotcha ya documentado en el README:** si el timestamp de una migración nueva queda antes que `InitialCreate` (pasa si el reloj de la máquina está mal o si se edita el archivo a mano), EF las aplica en el orden incorrecto y truena con `relation "X" does not exist`. La solución de emergencia está en el README, sección "Solución de problemas con migraciones" — solo bórrense migraciones si la base está vacía.

---

## 6. Sistema de licencias

- Campo `LicenseExpiresAt` (nullable) en `Tenant`. `IsLicenseActive` es una propiedad calculada (`LicenseExpiresAt == null || LicenseExpiresAt > DateTime.UtcNow`), no se guarda en DB.
- [`LicenseMiddleware.cs`](StyleForge.API/Middleware/LicenseMiddleware.cs) corre **después** de `UseAuthentication` y **antes** de `UseAuthorization` — por eso ya tiene `context.User` poblado. Lee el claim `tenantId`, busca el tenant, y si `!IsLicenseActive` corta la request con 403 antes de que llegue al controller.
- El registro (`/api/auth/register`) da automáticamente 30 días de trial (`AuthService.Register`).
- Renovar licencia (`POST /api/license/renew`) no usa JWT — usa el header `X-Master-Key` comparado con `Encoding.UTF8.GetBytes` + `CryptographicOperations.FixedTimeEquals` (para evitar timing attacks, ver [`LicenseController.cs`](StyleForge.API/Controllers/LicenseController.cs)).

---

## 7. CORS y el proxy de Vercel

El backend solo permite `http://localhost:4200` en `WithOrigins` — **esto es intencional**, no un bug. El frontend en producción (Vercel) nunca llama directo a Render: `vercel.json` tiene un rewrite `/api/:path*` → `https://styleforge-pjbu.onrender.com/api/:path*`, y el frontend usa rutas relativas (`apiUrl: '/api'`). Vercel reescribe la petición server-side, así que el navegador ve todo como same-origin y CORS nunca se activa en producción. Detalle completo en la sección [CORS del README](README.md).

**Consecuencia práctica:** como Render nunca ve la IP real del usuario final (ve la del proxy), cualquier lógica que dependa de `context.Connection.RemoteIpAddress` (como el rate limiting, sección siguiente) puede comportarse distinto a lo esperado en producción.

---

## 8. Rate limiting

Agregado en `Program.cs` con el rate limiter nativo de ASP.NET Core: política `"auth"`, **5 requests/minuto por IP**, sin cola (rechaza directo con `429`). Aplicada a todo `AuthController` (`register`, `login`, `login-client`) y a `LicenseController.RenewLicense` — los únicos endpoints que aceptan credenciales sin JWT previo.

**Pendiente conocido:** por el proxy de Vercel/Render (sección 7), `RemoteIpAddress` puede terminar siendo la IP del proxy en vez de la del usuario real, lo que haría que **todos los usuarios compartan el mismo balde de 5 requests/minuto**. La solución es agregar `ForwardedHeadersMiddleware` para leer `X-Forwarded-For` y particionar por la IP real — todavía no está hecho.

---

## 9. Testing

```bash
dotnet test StyleForge.Tests
```

- Usa EF Core **InMemory** (no Postgres real) vía [`DbContextHelper.CreateInMemory`](StyleForge.Tests/Helpers/DbContextHelper.cs).
- `ICurrentUserService` se mockea con Moq en cada test (`Mock<ICurrentUserService>`).
- **Cobertura actual: solo `AuthService` y `AppointmentService`.** Faltan tests de `ClientService`, `EmployeeService`, `ServiceService`, `TenantService`, `LicenseController`, y no hay tests de integración (`WebApplicationFactory`) que prueben `LicenseMiddleware` o el exception handler end-to-end.
- Ojo con `Assert.ThrowsAsync<T>` de xUnit: exige coincidencia **exacta** de tipo, no basta con que sea subclase. Si cambias qué excepción lanza un servicio, hay que actualizar el test o va a fallar aunque el comportamiento sea correcto.

---

## 10. CI/CD

- [`.github/workflows/ci.yml`](.github/workflows/ci.yml): en cada push/PR a `main` hace `dotnet restore` → `build` → `test`. No hay linting ni análisis estático adicional.
- Render está conectado al repo y **redespliega automático en cada push a `main`** — no hay ambiente de staging, todo lo que llega a `main` va directo a producción.
- El plan Free de Render se duerme tras 15 min de inactividad (cold start ~30s).

---

## 11. Variables de entorno / configuración

| Variable | Dónde se usa |
|---|---|
| `ConnectionStrings__DefaultConnection` | `Program.cs` → `AddDbContext` |
| `Jwt__Key`, `Jwt__Issuer`, `Jwt__Audience`, `Jwt__ExpireMinutes` | `JwtService`, `Program.cs` (validación del bearer) |
| `License__MasterKey` | `LicenseController.RenewLicense` |

No hay variable de entorno para CORS — el origen permitido está hardcodeado en `Program.cs` (ver sección 7 del porqué).

---

## 12. Deuda técnica conocida (orden de prioridad)

1. 🔴 **`TenantsController.Update` sin control de acceso** (sección 2) — cualquier usuario autenticado puede modificar cualquier tenant. Esto es lo primero que arreglaría.
2. 🟡 Rate limiting puede compartir balde entre usuarios distintos por el proxy (sección 8) — falta `ForwardedHeadersMiddleware`.
3. 🟡 Sin logging estructurado — solo hay un `LogError` puntual en el exception handler y en `/health`. Si algo falla en producción, la visibilidad es mínima.
4. 🟡 Cobertura de tests incompleta (sección 9).
5. 🟢 `Npgsql` y `Microsoft.Extensions.Caching.Memory` en 8.0.0 tienen advisories de seguridad conocidos (`GHSA-x9vc-6hfv-hg8c`, `GHSA-qj66-m88j-hmgj`) — revisar al actualizar paquetes.
6. 🟢 Sin refresh token (JWT expira en 60 min sin forma de renovar) ni recuperación de contraseña.
7. 🟢 `Class1.cs` en `Domain`, `Application` e `Infrastructure` son scaffolding vacío sin uso — se pueden borrar cuando alguien los vea.

---

## 13. Convenciones a mantener

- Los controllers que no capturan la excepción localmente dejan que suba al exception handler global — usar las excepciones de la tabla de la sección 4, no `Exception` genérica.
- Los DTOs de request terminan en `Request` (`CreateClientRequest`), los de respuesta en `Dto` (`ClientDto`).
- Los endpoints de listado devuelven `PagedResult<T>` con `page`/`pageSize` — hay que clampearlos en el controller (`Math.Clamp(pageSize, 1, 100)`), no confiar en el valor que mande el cliente.
- Las entidades que necesitan aislamiento por tenant heredan `BaseEntity` **y además** requieren el `HasQueryFilter` explícito en `AppDbContext` — un paso fácil de olvidar.
