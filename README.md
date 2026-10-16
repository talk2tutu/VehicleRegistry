# Vehicle Registry

A web application for managing vehicles and vehicle weight categories, built for the
CreditWorks Software Engineer assignment.

- **Backend:** C# / ASP.NET Core Web API on **.NET 10**, Entity Framework Core 10
- **Database:** Microsoft SQL Server 2022 (any SQL Server 2019+ works)
- **Frontend:** React + TypeScript (Vite)
- **Tests:** xUnit — 29 unit tests + 9 integration tests against a real SQL Server database

Design decisions, the category boundary rules and known limitations are in
**[docs/DESIGN.md](docs/DESIGN.md)**.

---

## 1. Required software

| Software | Version | Check with |
|---|---|---|
| .NET SDK | 10.0 | `dotnet --version` |
| Node.js | 22 LTS or later (Vite needs 20.19+) | `node --version` |
| SQL Server | 2019 or later — via Docker (recommended) or a local install / LocalDB | — |
| Docker Desktop | Only if running SQL Server in Docker | `docker --version` |
| Git | Any recent version | `git --version` |

All commands below are run from the repository root unless stated otherwise.

## 2. Start SQL Server

**Option A — Docker (macOS, Windows or Linux).** Choose your own strong password
(8+ characters with upper case, lower case, a number and a symbol):

```bash
docker run -e 'ACCEPT_EULA=Y' -e 'MSSQL_SA_PASSWORD=<YourStrong#Password1>' \
  -p 1433:1433 --name vehicle-sql --platform linux/amd64 \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

On Apple Silicon Macs, enable *Use Rosetta for x86_64/amd64 emulation* in
Docker Desktop → Settings → General. After a restart, use `docker start vehicle-sql`.

**Option B — an existing SQL Server or LocalDB (Windows).** No extra setup; just use
its connection string in the next step.

## 3. Configure the connection string

The connection string is **not stored in the repository**. Set it once with .NET user secrets
(stored in your user profile, outside the repo):

```bash
dotnet user-secrets set "ConnectionStrings:VehicleRegistry" \
  "Server=localhost,1433;Database=VehicleRegistry;User Id=sa;Password=<YourStrong#Password1>;TrustServerCertificate=True" \
  --project src/VehicleRegistry.Api
```

For LocalDB on Windows use:
`Server=(localdb)\MSSQLLocalDB;Database=VehicleRegistry;Trusted_Connection=True;TrustServerCertificate=True`

Alternatively, set the environment variable `ConnectionStrings__VehicleRegistry`.

> `TrustServerCertificate=True` is only for a local development database. Production would
> use a trusted certificate and a dedicated least-privilege login, not `sa`.

## 4. Create the database

**Nothing to do manually.** When the API starts in the Development environment it applies
the EF Core migrations, creating the database, tables, constraints and seed data
(5 manufacturers and the Light / Medium / Heavy categories).

To apply the migrations without starting the app (optional):

```bash
dotnet tool restore
dotnet ef database update --project src/VehicleRegistry.Api
```

## 5. Build

```bash
dotnet build
cd web && npm ci && npm run build && cd ..
```

`npm ci` installs the exact package versions from `package-lock.json`.

## 6. Run the application

Use two terminals.

**Terminal 1 — API** (http://localhost:5243):

```bash
dotnet run --project src/VehicleRegistry.Api --launch-profile http
```

**Terminal 2 — frontend** (http://localhost:5173):

```bash
cd web
npm ci        # first time only
npm run dev
```

Open **http://localhost:5173**. The Vite dev server forwards `/api` requests to the API,
so no CORS configuration is needed.

The OpenAPI description is available at http://localhost:5243/openapi/v1.json in Development.

## 7. Run the automated tests

SQL Server must be running and the connection string configured (step 3).

```bash
dotnet test
```

- **Unit tests** (`tests/VehicleRegistry.UnitTests`) — business rules only, no database.
- **Integration tests** (`tests/VehicleRegistry.IntegrationTests`) — run the real API in memory
  against a **separate database, `VehicleRegistry_IntegrationTests`**, which is dropped and
  rebuilt from the migrations on every run. Your development data is never touched.

Frontend checks:

```bash
cd web
npm run lint
npm run build    # includes the TypeScript type check
```

## 8. Project structure

```
src/
  VehicleRegistry.Core/          Domain model and business rules (no database or web code)
    Domain/                      Vehicle, Manufacturer, WeightCategory
    Rules/                       CategoryResolver, CategoryRules, VehicleRules, CategoryIcons
  VehicleRegistry.Api/           ASP.NET Core Web API
    Controllers/                 HTTP endpoints
    Services/                    VehicleService, CategoryService
    Contracts/                   Request/response DTOs
    Data/                        AppDbContext and EF Core migrations
tests/
  VehicleRegistry.UnitTests/         Business-rule tests
  VehicleRegistry.IntegrationTests/  API + SQL Server tests
web/                             React + TypeScript frontend
docs/DESIGN.md                   Design notes, assumptions and limitations
```

## 9. API overview

| Method | Endpoint | Purpose |
|---|---|---|
| GET | `/api/manufacturers` | List manufacturers |
| GET | `/api/vehicles?sortBy=owner\|manufacturer\|year\|weight&sortDir=asc\|desc` | List vehicles with their calculated category |
| GET | `/api/vehicles/{id}` | One vehicle |
| POST | `/api/vehicles` | Add a vehicle |
| GET | `/api/categories` | List categories with calculated ranges |
| GET | `/api/categories/icons` | Allowed icon keys |
| POST / PUT / DELETE | `/api/categories[/{id}]` | Create, edit, delete a category |

Validation failures return **400** with every problem listed
(`{ "errors": { "validation": [ ... ] } }`); missing records return **404**.

## 10. Troubleshooting

| Problem | Fix |
|---|---|
| "Could not load vehicles. Is the API running?" | Start the API (step 6, terminal 1). |
| API fails to start with a SQL connection error | Check SQL Server is running (`docker ps`) and the connection string password matches. |
| "No connection string found" when running tests | Complete step 3. |
| Port 5243 or 5173 already in use | Stop the other process, or change the port in `src/VehicleRegistry.Api/Properties/launchSettings.json` and `web/vite.config.ts`. |