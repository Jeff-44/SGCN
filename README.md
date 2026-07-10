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

## Certificate email and PDF configuration

Certificate PDFs are generated on demand by the authenticated endpoint:

```text
GET /api/v1/certificates/{id}/pdf
```

The endpoint returns `application/pdf` and does not persist generated files on the Railway filesystem. Citizens can only download certificates linked to their own requests; civil registry agents and administrators can download any accessible active certificate. Annulled certificates cannot be downloaded.

Configure these environment variables locally or in the Railway service:

| Variable | Description |
| --- | --- |
| `Email__Host` | SMTP server host |
| `Email__Port` | SMTP server port, commonly `587` or `465` |
| `Email__UserName` | SMTP user; leave both username and password empty only for a relay that does not require authentication |
| `Email__Password` | SMTP password |
| `Email__FromAddress` | Sender email address |
| `Email__FromName` | Sender display name, normally `SGCN` |
| `Email__EnableSsl` | `true` to require TLS (implicit TLS on port `465`, STARTTLS otherwise), or `false` for an unencrypted local relay |
| `Frontend__BaseUrl` | Public frontend URL used in notification links, without a trailing slash |

See [`.env.example`](.env.example) for placeholder values. Do not commit real SMTP credentials.

To test email delivery, configure a test SMTP inbox, generate a certificate from a citizen certificate request, and verify the message arrives at the email stored on that citizen's account. An SMTP failure is logged but does not roll back the certificate or its request status. Certificates generated directly from a birth record do not send an email.

To test PDF download, sign in as the citizen who created the request, open **Certificats**, and select **Télécharger PDF**. Repeat with another citizen to verify the API returns `403`, and with an administrator or civil registry agent to verify authorized access. The downloaded PDF should contain the certificate number, birth details, SGCN record ID, verification code, and issue date.

QuestPDF is configured with its Community license in `PdfService`. Confirm that the deploying organization is eligible for that license; otherwise select and obtain the appropriate QuestPDF license before production deployment.
