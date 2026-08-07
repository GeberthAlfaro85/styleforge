# StyleForge API

API REST para gestión de salones de belleza. Permite administrar clientes, empleados, servicios y citas, con soporte multi-tenant y autenticación por roles.

---

## Tecnologías

- .NET 8 / ASP.NET Core
- Entity Framework Core + PostgreSQL
- JWT Authentication (BCrypt para contraseñas)
- Arquitectura limpia: Domain → Application → Infrastructure → API

---

## Requisitos previos

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- PostgreSQL 14+
- Un cliente HTTP (Swagger UI incluido, Postman, curl)

---

## Configuración

### 1. Base de datos

Crea una base de datos en PostgreSQL:

```sql
CREATE DATABASE styleforge_db;
```

### 2. Variables de configuración

Edita `StyleForge.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=styleforge_db;Username=postgres;Password=TU_PASSWORD"
  },
  "Jwt": {
    "Key": "CLAVE_SECRETA_MINIMO_32_CARACTERES_AQUI",
    "Issuer": "StyleForge",
    "Audience": "StyleForgeUsers",
    "ExpireMinutes": 60
  }
}
```

> La `Jwt:Key` debe tener mínimo 32 caracteres. Cámbiala antes de ir a producción.

### 3. Aplicar migraciones

Desde la carpeta `StyleForge.Infrastructure`:

```bash
dotnet ef database update --startup-project ..\StyleForge.API
```

---

#### Solución de problemas con migraciones

**Error: `relation "X" does not exist`**

Ocurre cuando las migraciones tienen timestamps desordenados (una migración tardía tiene un timestamp anterior a `InitialCreate`), lo que hace que EF Core las aplique en el orden incorrecto. Solución: eliminar todas las migraciones y recrear una sola desde el modelo actual.

```bash
# Desde StyleForge.Infrastructure
Remove-Item Migrations\202605*
Remove-Item Migrations\AppDbContextModelSnapshot.cs
dotnet ef migrations add InitialCreate --startup-project ..\StyleForge.API
dotnet ef database update --startup-project ..\StyleForge.API
```

> Solo hacer esto si la base de datos está vacía o es nueva. Si ya tiene datos, analizar primero qué migraciones faltan.

---

**Error: `Host desconocido` o fallo de DNS al conectar con Supabase**

Las conexiones directas a Supabase (`db.xxx.supabase.co`) usan **solo IPv6**. Si tu red es IPv4, usa el **Session Pooler** en `appsettings.json`:

```json
"DefaultConnection": "Host=aws-1-us-east-2.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.{PROJECT-REF};Password={PASSWORD};SSL Mode=Require;Trust Server Certificate=true"
```

El host y el `USERNAME` exactos están en el dashboard de Supabase → **Get connected → Pooler settings**.

### 4. Ejecutar

```bash
cd StyleForge.API
dotnet run
```

La API queda disponible en `http://localhost:5087`. El Swagger UI en `http://localhost:5087/swagger`.

> **Producción:** `https://styleforge-pjbu.onrender.com` — Swagger en `https://styleforge-pjbu.onrender.com/swagger`

---

## Deploy en Render

El proyecto incluye un `Dockerfile` en la raíz. Para desplegar en Render:

1. Crea un **Web Service** en [render.com](https://render.com) conectado al repositorio de GitHub.
2. Configura:
   - **Language:** Docker
   - **Branch:** main
   - **Instance Type:** Free
3. Agrega las siguientes **Environment Variables**:

| Variable | Descripción |
|---|---|
| `ConnectionStrings__DefaultConnection` | Connection string de Supabase (Session Pooler) |
| `Jwt__Key` | Clave secreta JWT (mínimo 32 caracteres) |
| `Jwt__Issuer` | `StyleForge` |
| `Jwt__Audience` | `StyleForgeUsers` |
| `License__MasterKey` | Clave maestra para renovar licencias (inventar una clave segura) |

4. Haz clic en **Deploy Web Service**.

Render redespliega automáticamente en cada push a `main`.

> **Nota:** El plan Free se duerme tras 15 min de inactividad. La primera petición después tarda ~30 seg (cold start).

> **URL de producción:** `https://styleforge-pjbu.onrender.com` — Swagger: `https://styleforge-pjbu.onrender.com/swagger`

---

## CORS

El backend solo permite el origen `http://localhost:4200` (ver `Program.cs`). Esto es intencional: el frontend en Vercel **no llama directo** a Render. En `vercel.json` hay un rewrite:

```json
{ "source": "/api/:path*", "destination": "https://styleforge-pjbu.onrender.com/api/:path*" }
```

y `environment.prod.ts` apunta a `apiUrl: '/api'` (ruta relativa). Vercel reescribe la petición server-side antes de que llegue al navegador, así que desde la perspectiva del browser todo es same-origin y CORS nunca se activa en producción.

La whitelist de `http://localhost:4200` solo protege contra quien le pegue **directo** al dominio de Render (por ejemplo, un `environment.ts` de dev mal configurado). Si en algún momento algo necesita llamar directo a Render desde el navegador, hay que agregar ese origen a `WithOrigins(...)`.

---

## Arquitectura

```
StyleForge.Domain          → Entidades (User, Client, Appointment, Service)
StyleForge.Application     → DTOs, interfaces de servicios
StyleForge.Infrastructure  → EF Core, implementaciones, JWT
StyleForge.API             → Controllers, configuración
```

Cada módulo sigue el mismo patrón:
1. Entidad en `Domain`
2. DTO + interfaz en `Application`
3. Implementación en `Infrastructure`
4. Controller en `API`

---

## Multi-tenancy

Cada salón que se registra crea un `Tenant` independiente. Todos los datos (clientes, citas, servicios, empleados) están aislados por `TenantId` usando global query filters de EF Core. Un salón nunca puede ver los datos de otro.

---

## Roles

| Rol | Descripción |
|-----|-------------|
| `Admin` | Dueño/administrador del salón. Acceso total. |
| `User` | Empleado del salón. Puede ver citas. |
| `Client` | Cliente del salón. Solo ve y crea sus propias citas. |

---

## Autenticación

Todos los endpoints (excepto register y login) requieren un header:

```
Authorization: Bearer {token}
```

El token se obtiene al registrarse o hacer login y expira en 60 minutos.

---

## Endpoints

### Auth — `POST /api/auth`

#### Registrar salón (crea tenant + admin)
```http
POST /api/auth/register
Content-Type: application/json

{
  "companyName": "Mi Salón",
  "name": "Juan Pérez",
  "email": "juan@salon.com",
  "password": "MiPassword123"
}
```

#### Login — Admin o empleado
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "juan@salon.com",
  "password": "MiPassword123"
}
```

#### Login — Cliente
```http
POST /api/auth/login-client
Content-Type: application/json

{
  "email": "cliente@gmail.com",
  "password": "ClientePass123"
}
```

Respuesta de todos los login:
```json
{
  "token": "eyJhbGci...",
  "user": {
    "id": "...",
    "name": "Juan Pérez",
    "email": "juan@salon.com",
    "role": "Admin",
    "tenantId": "..."
  }
}
```

---

### Clientes — `GET|POST|PUT|DELETE /api/clients`

> Requiere token. Create/Update/Delete solo Admin.

#### Listar / buscar clientes
```http
GET /api/clients?search=Juan&page=1&pageSize=20
```

#### Crear cliente
```http
POST /api/clients
Authorization: Bearer {token}

{
  "name": "Ana García",
  "phone": "50412345678",
  "email": "ana@gmail.com",
  "password": "Ana123"       // opcional — permite que el cliente haga login
}
```

#### Actualizar cliente
```http
PUT /api/clients/{id}
Authorization: Bearer {token}

{
  "name": "Ana García López",
  "phone": "50412345678",
  "email": "ana@gmail.com"
}
```

#### Eliminar cliente
```http
DELETE /api/clients/{id}
Authorization: Bearer {token}
```

---

### Empleados — `GET|POST|PUT|DELETE /api/employees`

> Solo Admin.

#### Listar empleados
```http
GET /api/employees?page=1&pageSize=20
Authorization: Bearer {token}
```

#### Crear empleado
```http
POST /api/employees
Authorization: Bearer {token}

{
  "name": "Sofia Castro",
  "email": "sofia@salon.com",
  "password": "Sofia123"
}
```
El empleado se crea con rol `User` dentro del mismo tenant del Admin. Puede hacer login con `POST /api/auth/login`.

#### Actualizar / Eliminar
```http
PUT /api/employees/{id}
DELETE /api/employees/{id}
```

---

### Servicios — `GET|POST|PUT|DELETE /api/services`

> Listar: cualquier usuario autenticado. Create/Update/Delete: solo Admin.

#### Listar servicios
```http
GET /api/services?page=1&pageSize=20
Authorization: Bearer {token}
```

#### Crear servicio
```http
POST /api/services
Authorization: Bearer {token}

{
  "name": "Corte de cabello",
  "price": 250.00,
  "durationMinutes": 45
}
```

#### Actualizar / Eliminar
```http
PUT /api/services/{id}
DELETE /api/services/{id}
```

---

### Citas — `/api/appointments`

#### Ver todas las citas del salón (Admin y empleados)
```http
GET /api/appointments?page=1&pageSize=20
Authorization: Bearer {token Admin o User}
```

#### Ver mis citas (solo Clientes)
```http
GET /api/appointments/mine?page=1&pageSize=20
Authorization: Bearer {token Client}
```

#### Crear cita
```http
POST /api/appointments
Authorization: Bearer {token}

{
  "clientId": "...",         // requerido si quien crea es Admin o empleado
                             // omitir si quien crea es el propio Cliente
  "serviceId": "...",
  "staffId": "...",
  "scheduledAt": "2026-06-15T14:00:00Z",
  "notes": "Tinte rubio"    // opcional
}
```

> Cuando un cliente crea su propia cita, el `clientId` se toma automáticamente del token JWT.

#### Actualizar estado de una cita (solo Admin)
```http
PUT /api/appointments/{id}/status
Authorization: Bearer {token Admin}

{
  "status": "Confirmed"   // Pending | Confirmed | Cancelled | Completed
}
```

---

## Respuesta de paginación

Todos los endpoints de listado devuelven:

```json
{
  "items": [...],
  "totalCount": 50,
  "page": 1,
  "pageSize": 20,
  "totalPages": 3,
  "hasNext": true,
  "hasPrev": false
}
```

---

## Respuestas de error

| Código | Significado |
|--------|-------------|
| 400 | Datos inválidos o regla de negocio (ej: email duplicado) |
| 401 | Sin token o credenciales incorrectas |
| 403 | Token válido pero sin permisos para esa acción |
| 404 | Recurso no encontrado |

Formato del error:
```json
{
  "message": "Client not found"
}
```

---

## Sistema de Licencias

### ¿Dónde se guarda?

El campo `LicenseExpiresAt` (timestamp nullable) se almacena directamente en la tabla `Tenants`:

```sql
SELECT "Id", "Name", "LicenseExpiresAt" FROM "Tenants";
```

| Campo | Tipo | Descripción |
|---|---|---|
| `LicenseExpiresAt` | `timestamp with time zone` (nullable) | Fecha de expiración. `NULL` = licencia permanente. |

### Comportamiento

- Al registrarse, cada tenant recibe **30 días de trial automáticamente**.
- Cuando la licencia expira, cualquier request autenticado retorna `403 Forbidden`.
- Los endpoints de login (`/api/auth/login`, `/api/auth/login-client`) siguen funcionando aunque la licencia esté expirada.

### Endpoints

#### Consultar estado de licencia
```http
GET /api/license
Authorization: Bearer {token}
```
Respuesta:
```json
{
  "isActive": true,
  "expiresAt": "2026-06-25T00:00:00Z",
  "daysRemaining": 30,
  "status": "Activa"
}
```
`status` puede ser: `"Activa"`, `"Expirada"` o `"Permanente"` (cuando `LicenseExpiresAt` es null).

#### Renovar licencia
No requiere JWT. Usa una clave maestra en el header `X-Master-Key` (configurada en variables de entorno como `License__MasterKey`).

Si la licencia aún está activa, extiende desde la fecha de expiración actual. Si ya expiró, extiende desde hoy.

```http
POST /api/license/renew
X-Master-Key: {tu-master-key}
Content-Type: application/json

{
  "tenantId": "uuid-del-tenant",
  "days": 30
}
```
Respuesta:
```json
{
  "message": "Licencia renovada por 30 días.",
  "tenantId": "...",
  "tenantName": "Mi Salón",
  "license": {
    "isActive": true,
    "expiresAt": "2026-07-25T00:00:00Z",
    "daysRemaining": 30,
    "status": "Activa"
  }
}
```

---

## Flujo típico de uso

```
1. POST /api/auth/register         → El salón crea su cuenta
2. POST /api/employees             → Admin agrega empleados
3. POST /api/services              → Admin carga el catálogo de servicios
4. POST /api/clients               → Admin registra clientes (con password opcional)
5. POST /api/auth/login-client     → Cliente hace login
6. POST /api/appointments          → Admin o cliente crea una cita
7. PUT  /api/appointments/{id}/status → Admin confirma la cita
8. GET  /api/appointments/mine     → Cliente ve su cita confirmada
```
