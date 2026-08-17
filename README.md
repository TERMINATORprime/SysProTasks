# SYSPRO — Legacy Order Ingestion & Management

A small .NET 10 solution that ingests daily legacy order CSVs into SQL Server (with
order versioning + an import audit) and exposes an HTTP API to create and query orders.

- **SysPro.CLI** — Task 1: reads a folder of daily CSVs and ingests them.
- **SysPro.API** — Task 2/3: create an order via JSON, fetch by id, customer totals.
- **SysPro.DB** — EF Core `DbContext`, migrations, and embedded SQL (table types + stored procs).
- **SysPro.Application / SysPro.Domain / SysPro.Core** — ingestion logic, entities, DTOs.

---

## Prerequisites

- **.NET SDK 10** — check with `dotnet --version` (built against `10.0.x`).
- **SQL Server 2022** reachable on `localhost:1433`. The steps below use Docker; any
  SQL Server instance works (SQL Express / a hosted instance / LocalDB on Windows).
- **Docker** — needed to run the integration test (it starts a throwaway SQL Server via
  Testcontainers), and optionally to host the app's own SQL Server via the container below.

---

## 1. Start SQL Server

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Your_Str0ng!Passw0rd" \
  -p 1433:1433 --name syspro-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

The SA password must meet SQL Server complexity rules (8+ chars, with upper, lower,
digit, and a symbol). On Windows/PowerShell, run the same command on one line.

---

## 2. Configure the connection string

The app resolves the connection string from the key **`ConnectionStrings:Default`**.

> **Note:** the CLI reads the environment variable **only** — it does not read
> user-secrets or `appsettings.json`. The env-var approach below is the one path that
> works for the CLI, the API, and EF migrations, so prefer it.

### Environment variable (works everywhere)

The double underscore `__` maps to the config `:` separator, so
`ConnectionStrings__Default` → `ConnectionStrings:Default`.

**Linux / macOS (bash)** — single-quote the value so the shell doesn't expand `$`, `!`, etc.:

```bash
export ConnectionStrings__Default='Server=localhost,1433;Database=SysproOrders;User Id=sa;Password=Your_Str0ng!Passw0rd;TrustServerCertificate=True;Encrypt=True'
```

**Windows (PowerShell):**

```powershell
$env:ConnectionStrings__Default = 'Server=localhost,1433;Database=SysproOrders;User Id=sa;Password=Your_Str0ng!Passw0rd;TrustServerCertificate=True;Encrypt=True'
```

Run every command below **in the same shell** so the variable is visible.

### Alternative: user-secrets (API + migrations only)

If you prefer to keep the secret out of your shell history, the **API** project has a
`UserSecretsId`, so you can store it there instead. Note the key is `ConnectionStrings:Default`:

```bash
dotnet user-secrets set "ConnectionStrings:Default" 'Server=localhost,1433;Database=SysproOrders;User Id=sa;Password=Your_Str0ng!Passw0rd;TrustServerCertificate=True;Encrypt=True' --project SysPro.API
```

This covers the API and EF migrations (they run through the API startup project). The
**CLI still needs the environment variable** from the previous section.

---

## 3. Create the database schema

Restore the local EF tool, then apply the migration. The migration creates the tables
**and** the user-defined table types + stored procedures (they're embedded SQL run inside
the migration), so a single `database update` sets up everything:

```bash
dotnet tool restore
dotnet ef database update --project SysPro.DB --startup-project SysPro.API
```

---

## 4. Ingest the CSVs (Task 1 — CLI)

Sample data lives in `csvData/` at the repo root (`orders_day_1.csv` … `orders_day_3.csv`).
Pass the folder to ingest as the first argument:

```bash
dotnet run --project SysPro.CLI -- "/absolute/path/to/SysproTask/csvData"
```

With no argument, the CLI looks for a `csvData` folder next to the built executable
(`SysPro.CLI/bin/.../csvData`). Passing the path explicitly is the most predictable.

Each file prints an `Applied` / `Invalid` count and writes an import-audit row. The CLI
waits on `Console.ReadLine()` at the end — press **Enter** to exit.

> **Rider:** set the folder in *Run → Edit Configurations → SysPro.CLI → Program arguments*.
> If the config is a launch-profile type and that field is disabled, add `commandLineArgs`
> to `SysPro.CLI/Properties/launchSettings.json` instead. Quote paths that contain spaces.

---

## 5. Run the API (Task 2/3)

```bash
dotnet run --project SysPro.API
```

The default (`http`) profile serves at **http://localhost:5071**. In Development the
Scalar API reference is at:

```
http://localhost:5071/scalar/v1
```

For HTTPS (https://localhost:7278), run with the https profile:

```bash
dotnet run --project SysPro.API --launch-profile https
```

### Endpoints

| Method | Route                          | Description                          |
|--------|--------------------------------|--------------------------------------|
| GET    | `/api/orders`                  | All orders (latest version)          |
| GET    | `/api/orders/{id:guid}`        | One order + its lines by internal id |
| GET    | `/api/orders/byExternal`       | Lookup by external id(s)             |
| GET    | `/api/orders/by-date-range`    | Orders within an order-date range    |
| POST   | `/api/orders`                  | Create/update orders from JSON       |

---

## 6. Run the tests

Tests live in `SysPro.Tests` (xUnit). The suite splits into fast in-process unit tests and
one integration test tagged `[Trait("Category", "Integration")]`.

**Unit tests only** — run against fakes, need no database or Docker:

```bash
dotnet test SysPro.Tests/SysPro.Tests.csproj --filter "Category!=Integration"
```

**Integration test only** — spins up a throwaway SQL Server via **Testcontainers**, applies
the EF migrations to it, ingests the sample CSVs, and asserts the committed state. It needs
**Docker running**, but does *not* use `ConnectionStrings__Default` or your own SQL Server —
the container is created and torn down by the test:

```bash
dotnet test SysPro.Tests/SysPro.Tests.csproj --filter "Category=Integration"
```

**Everything** (requires Docker, for the integration test):

```bash
dotnet test SysPro.Tests/SysPro.Tests.csproj
```

---

## Configuration notes

- Connection-string key is **`ConnectionStrings:Default`** — the `[From Secret]` value in
  `appsettings.json` is a placeholder that the env var (or a user-secret) overrides at runtime.
- Env var `ConnectionStrings__Default` overrides `appsettings.json` and is required by the CLI.
- `TrustServerCertificate=True` is needed for SQL Server's self-signed dev certificate.
