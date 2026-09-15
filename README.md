# Leave Requests

Internal HR module for managing employee leave requests. The API owns the `LeaveRequests` table in a local SQL Server database; employee data is fetched on-demand from an external HR system (`dummyjson.com/users`).

## Stack

- **Backend** — ASP.NET Core 8 Web API, EF Core 8 (SQL Server), HttpClientFactory, in-memory caching, Swagger
- **Frontend** — Blazor WebAssembly (.NET 8)
- **Database** — SQL Server (LocalDB works fine for dev)

## Running locally

### 1. Database

Option A — run the SQL script directly:

```bash
sqlcmd -S ASMAA_KHALED -E -i database/init.sql
```

Option B — apply the EF Core migration (schema only, no seed data):

```bash
cd src/LeaveRequests.API
dotnet ef database update
```

The default connection string targets `(localdb)\mssqllocaldb`. To use a different instance, edit `ConnectionStrings.DefaultConnection` in `src/LeaveRequests.API/appsettings.json`.

### 2. API

```bash
cd src/LeaveRequests.API
dotnet run
```

API listens on `http://localhost:5050`. Swagger UI: `http://localhost:5050/swagger`.

### 3. Frontend

```bash
cd src/LeaveRequests.Web
dotnet run
```

Open the URL printed by the runtime (usually `http://localhost:5xxx`). The app expects the API on `http://localhost:5050` — if you change the API port, update `Program.cs` in `LeaveRequests.Web` accordingly.

## API reference

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/leave-requests` | List, filterable by `status` / `employeeId`, paginated (`page`, `pageSize`) |
| GET | `/api/leave-requests/{id}` | Single request with employee details |
| POST | `/api/leave-requests` | Create — validates employee exists, checks date range |
| PUT | `/api/leave-requests/{id}/status` | Approve / Reject (Pending → Approved or Rejected only) |
| DELETE | `/api/leave-requests/{id}` | Delete — only while Pending |
| GET | `/api/employees` | Proxy to external HR API, supports `?search=` |

See `requests.http` for ready-to-run examples.

## Design decisions

**No database FK for `EmployeeId`** — employees are not stored locally; they live in a third-party system. A database FK requires a local table to reference. Instead, employee existence is validated in code by calling the external API before any insert. This is noted wherever the column is declared.

**HttpClientFactory** — `EmployeeService` receives a typed `HttpClient` configured with a base address and a 10-second timeout. This avoids socket exhaustion from `new HttpClient()` per request and centralises timeout policy.

**In-memory employee cache** — the full employee list is cached for 5 minutes. Per-ID lookups bypass the cache (already fast single-record responses). Search queries always hit the live API.

**Status transitions** — only `Pending → Approved` and `Pending → Rejected` are valid. Any other transition returns 409 Conflict. Deletion is similarly restricted to `Pending` requests.

**Error shape** — all error responses use `{ "error": "..." }` for consistency.

**Raw SQL** — none. All queries use EF Core with parameterised LINQ. The check constraints in the migration use string literals for allowed values only (not user input).

## What I'd do next with more time

- Add JWT auth with an HR role guard on approve/reject endpoints
- Expand status transition rules if the business requires (allow HR to cancel approved requests)
- Add an integration test project covering the status transition logic and the employee-validation path
- Containerise with Docker Compose (SQL Server + API + WASM served via nginx)
- Surface paging controls and a date-range filter in the UI
