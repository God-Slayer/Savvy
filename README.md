# Savvy Timesheets API

Savvy Timesheets is a .NET 8 Web API for managing practices, clinician shifts, timesheets, and payment runs. It is an API-only service; Swagger and Postman can be used to exercise the endpoints.

## Prerequisites

- .NET 8 SDK
- Visual Studio 2022 (or another .NET 8-compatible IDE)
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) or SQL Server

## Run locally

1. Clone the repository and open `Savvy.Timesheets.sln`.
2. Confirm the connection string and development seed settings in `Savvy.Api/appsettings.Development.json`.
3. Set `Savvy.Api` as the startup project.
4. Start the `https` profile, or run:

   ```powershell
   dotnet run --project Savvy.Api --launch-profile https
   ```

5. Open Swagger at [https://localhost:7026/swagger](https://localhost:7026/swagger).

The API applies pending Entity Framework Core migrations at startup and seeds a small development dataset (one practice, Admin, PracticeManager, Clinician, shifts, timesheets, and payment data). The seeded users share the development password configured under `DevelopmentSeed:DemoPassword`.

The HTTP profile is available at `http://localhost:5004` if HTTPS is not required.

## Test

Run the automated tests from the repository root:

```powershell
dotnet test Savvy.Tests/Savvy.Tests.csproj
```

The included `Savvy.postman_collection.json` allows for testing via Postman and provides requests and variables for login, shifts, timesheets, payment runs, reporting, and status filtering.

## Project structure

- `Savvy.Domain` — entities and domain enums.
- `Savvy.Application` — business rules, services, DTOs, calculation logic, and repository contracts.
- `Savvy.Infrastructure` — EF Core, SQL Server persistence, repositories, authentication implementation, migrations, reporting, and development seeding.
- `Savvy.Api.Controllers` — HTTP controllers and API endpoint behavior.
- `Savvy.Api` — application composition, middleware, authentication, Swagger, and startup.
- `Savvy.Tests` — automated regression and integration coverage.
