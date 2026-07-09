# SGCN

SGCN is an ASP.NET Core Web API starter using a lightweight Clean Architecture layout, EF Core Code First, PostgreSQL, and ASP.NET Core Identity.

## Projects

- `SGCN.Domain`: entities, enums, base domain models.
- `SGCN.Application`: DTOs, interfaces, commands, queries, contracts, application logic.
- `SGCN.Infrastructure`: EF Core DbContext, repositories, Identity configuration, external service placeholders.
- `SGCN.Api`: controllers, middleware, authentication setup, Swagger, configuration, dependency injection.

## Initial Roles

Identity roles are seeded on application startup when the configured database is available:

- `Administrateur`
- `AgentEtatCivil`
- `Medecin`
- `Citoyen`

## Run Locally

1. Start PostgreSQL:

```bash
docker compose up -d postgres
```

The default development database listens on `localhost:5433` and uses:

- Database: `sgcn`
- Username: `postgres`
- Password: `Sgcn@2026Dev!`

2. Update the `DefaultConnection` value in `SGCN.Api/appsettings.json` or `SGCN.Api/appsettings.Development.json` if you change these values.
3. Update the `Jwt` values before using real authentication outside local development.
4. Restore packages:

```bash
dotnet restore
```

5. Create and apply the first EF Core migration:

```bash
dotnet ef migrations add InitialCreate --project SGCN.Infrastructure --startup-project SGCN.Api
dotnet ef database update --project SGCN.Infrastructure --startup-project SGCN.Api
```

6. Run the API:

```bash
dotnet run --project SGCN.Api
```

Swagger is enabled in development at `/swagger`. A basic health endpoint is available at `/health` and `/api/v1/health`.

If `dotnet ef` is not installed, install it with:

```bash
dotnet tool install --global dotnet-ef
```
