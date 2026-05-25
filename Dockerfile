FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["StyleForge.API/StyleForge.API.csproj", "StyleForge.API/"]
COPY ["StyleForge.Application/StyleForge.Application.csproj", "StyleForge.Application/"]
COPY ["StyleForge.Domain/StyleForge.Domain.csproj", "StyleForge.Domain/"]
COPY ["StyleForge.Infrastructure/StyleForge.Infrastructure.csproj", "StyleForge.Infrastructure/"]
RUN dotnet restore "StyleForge.API/StyleForge.API.csproj"
COPY . .
RUN dotnet publish "StyleForge.API/StyleForge.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "StyleForge.API.dll"]
