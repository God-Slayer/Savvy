# Solution Notes

## Architecture

The solution follows a layered Clean Architecture style. Domain entities and enums are kept independent of infrastructure. Application services contain authorization checks, business workflows, idempotency rules, and payment calculations. Infrastructure implements persistence and authentication details through EF Core, repositories, a Unit of Work, JWT generation, migrations, and seed data. The API projects provide HTTP controllers, Swagger, configuration, and middleware.

This separation keeps business rules testable and prevents controllers from becoming responsible for database or calculation logic. It also allows persistence or hosting details to change without rewriting the domain workflows.

## Sensitive data and safe errors

- Passwords are hashed with ASP.NET Core Identity's password hasher; plaintext passwords are never persisted.
- All non-public endpoints require JWT bearer authentication, with role and practice-scope checks for Admin, PracticeManager, and Clinician operations.
- JWTs contain only the identity, role, and practice scope needed for authorization and are signed with a configured symmetric key.
- Business references protect timesheet and payment-run submissions from creating duplicates. Conflicting reuse is handled as a business error rather than silently creating another record.
- UTC timestamps are used for worked-time calculations and inclusive date-range filtering. Monetary values are rounded to two decimal places using the assignment's required midpoint behavior.
- Centralized operational middleware adds correlation IDs, logs unhandled exceptions, and returns a consistent problem response. Internal exception details are not exposed to API callers; the correlation ID can be used to locate the server-side log entry.
- `appsettings.Development.json` contains demo-only local values so the repository can be cloned and run easily. These values must not be used for production deployments.

## Azure App Service deployment

1. Build and publish the `Savvy.Api` project for the target .NET 8 runtime.
2. Create an Azure App Service with a managed SQL database (Azure SQL Database is the production equivalent of LocalDB).
3. Configure App Service application settings/environment variables rather than committing production values. ASP.NET Core configuration maps settings such as:

   ```text
   ConnectionStrings__TimesheetsDatabase
   Jwt__Key
   Jwt__Issuer
   Jwt__Audience
   Jwt__ExpiryMinutes
   ```

4. Store the database connection string and JWT signing key in Azure Key Vault. Use the App Service managed identity and Key Vault references or deployment-time secret injection so the application receives secrets without source-code changes.
5. Set `ASPNETCORE_ENVIRONMENT` to the appropriate deployment environment and configure HTTPS-only access, TLS, and restricted database networking.
6. Apply migrations through a controlled deployment step or release job. For production, seed data should be replaced with an explicit provisioning process rather than the local development seed.
7. Enable App Service/Application Insights logging and retain correlation IDs so operational failures can be investigated without returning sensitive exception details to clients.

The same deployment can use CI/CD from GitHub: restore, build, test, publish, apply the approved database migration, and deploy the published API artifact to App Service.
