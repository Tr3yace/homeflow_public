# HomeFlow

## Overview

HomeFlow is a household information platform designed to aggregate and display useful home-related information through a central API.

Current functionality includes:

- South Gloucestershire bin collection integration
- On-demand bin collection refresh in the API
- Dashboard UI
- Database-backed logging with level filtering and retention
- REST API endpoints
- Future Pico dashboard support

---

## Architecture

```text
HomeFlow.Web.UI (dashboard)
       ↓
HomeFlow.Service API
       ↓  (refreshes on demand when data is stale)
South Gloucestershire API
       ↓
SQL Database

Future Pico
       ↓
HomeFlow.Service API
```

The Web UI does not communicate directly with the database. All consumer applications should obtain data through the API.

---

## Solution Structure

```text
HomeFlow
│
├── HomeFlow.Data
│   ├── Entities
│   ├── DTOs
│   ├── Settings
│   └── ApplicationDbContext
│
├── HomeFlow.Service
│   ├── Services (incl. BinRefreshService)
│   ├── Controllers
│   ├── Helpers
│   └── Extensions
│
├── HomeFlow.Web.UI
│   ├── Razor Pages
│   ├── ViewModels
│   ├── Mappers
│   └── UI Assets
│
└── HomeFlow.Worker
    └── Bin Refresh Worker (local use only, not deployed)
```

---

## Technologies

- .NET 10
- ASP.NET Core
- Razor Pages
- Entity Framework Core
- SQL Server
- HttpClient

---

## Getting Started

### Prerequisites

Install:

- .NET 10 SDK
- Visual Studio 2026 (or later)
- SQL Server LocalDB or SQL Server Express
- Entity Framework Core tools

Verify:

```bash
dotnet --version
```

Expected:

```text
10.x.x
```

---

### Clone Repository

```bash
git clone <repository-url>
cd HomeFlow
```

---

### Restore Packages

```bash
dotnet restore
```

---

### Create Database

Apply migrations:

```bash
dotnet ef database update --project HomeFlow.Data
```

Or run from Visual Studio:

```text
Package Manager Console

Update-Database
```

Verify tables exist:

```text
BinCollections
RegisteredUprns
Logs
```

---

### Add Initial UPRN

Insert at least one UPRN into:

```text
RegisteredUprns
```

Example:

```sql
INSERT INTO RegisteredUprns
(
    Id,
    Uprn
)
VALUES
(
    NEWID(),
    'YOUR_UPRN'
);
```

Only registered UPRNs trigger a call to the council API.

---

### Run the API and Dashboard

Set startup project:

```text
HomeFlow.Web.UI
```

Run:

```text
F5
```

Verify Swagger:

```text
https://localhost:44351/swagger
```

Verify Dashboard:

```text
https://localhost:44351/Dashboard
```

The first dashboard load refreshes the bin data from the council API if it is missing or more than 24 hours old.

---

### Verify Refresh Process

Check:

```sql
SELECT
    ServiceName,
    NextCollection,
    LastUpdated
FROM BinCollections;
```

Confirm:

- Records are inserted
- `LastUpdated` changes when the dashboard is loaded after the data has gone stale

Check logs:

```sql
SELECT TOP 20 *
FROM Logs
ORDER BY Timestamp DESC;
```

Expected entries after a refresh:

```text
Saving entities: [...]
Successfully ingested 4 bin records for UPRN YOUR_UPRN
```

---

## Configuration

### HomeFlow.Web.UI

```json
{
  "ConnectionStrings": {
    "DefaultConnection": ""
  },

  "DashboardSettings": {
    "DefaultUprn": ""
  },

  "ApiSettings": {
    "BaseUrl": "https://localhost:44351",
    "AdminKey": ""
  },

  "BinRefreshSettings": {
    "MaxAgeHours": 24,
    "FailureCooldownMinutes": 15
  },

  "LogSettings": {
    "MinimumLevel": "Information",
    "RetentionDays": 30
  }
}
```

| Setting | Purpose |
| --- | --- |
| `DashboardSettings:DefaultUprn` | UPRN shown on the dashboard. **Secret**: a UPRN identifies a property, so never commit it. The dashboard is empty until it is set. |
| `ApiSettings:BaseUrl` | URL the dashboard page uses to call the API. Must be the site's own URL in each environment. |
| `ApiSettings:AdminKey` | **Secret.** Required in the `X-Api-Key` header for `POST /api/bins/{uprn}/fetchfromapi`. When empty, that endpoint is disabled. |
| `BinRefreshSettings:MaxAgeHours` | Stored data older than this is refreshed on the next request. |
| `BinRefreshSettings:FailureCooldownMinutes` | After a failed refresh, wait this long before calling the council API again. |
| `LogSettings:MinimumLevel` | `Debug`, `Information`, `Warning` or `Error`. Lower levels are not written to the `Logs` table. |
| `LogSettings:RetentionDays` | Log entries older than this are deleted (checked at most once a day). `0` keeps them forever. |

In Azure App Service, override these under Configuration → Application settings using `__` as the separator, for example `ApiSettings__BaseUrl`.

### Secrets

Keep the UPRN and admin key out of `appsettings.json`.

Locally, use .NET user secrets (loaded in the Development environment):

```bash
dotnet user-secrets set "DashboardSettings:DefaultUprn" "YOUR_UPRN" --project HomeFlow.Web.UI
```

```bash
dotnet user-secrets set "ApiSettings:AdminKey" "a-long-random-value" --project HomeFlow.Web.UI
```

In Azure, add `DashboardSettings__DefaultUprn` and `ApiSettings__AdminKey` as App Service application settings.

---

## Bin Refresh

Bin data is refreshed **on demand by the API**, not by a background worker. This is because the app is hosted on an Azure App Service **F1** plan, which has no Always On: the site unloads when idle, so WebJobs and background timers cannot run reliably.

`GET /api/bins/{uprn}/dashboard` and `GET /api/bins/{uprn}/next` call `IBinRefreshService.EnsureFreshAsync(uprn)` before reading data.

```text
Request arrives
      ↓
Is stored data older than MaxAgeHours?
      ↓ yes                         ↓ no
In failure cooldown? ── yes ──→ Serve stored data
      ↓ no
Is the UPRN registered? ── no ──→ Serve stored data
      ↓ yes
Wait for this UPRN's refresh lock
(other requests for the same UPRN wait here)
      ↓
Call South Gloucestershire API (10s timeout)
      ↓
Replace BinCollections rows for the UPRN
      ↓
Serve stored data
```

The refresh never fails the request: if the council API is slow or down, the stored data is returned and the next attempt waits for the cooldown.

### Worker (local use only)

`HomeFlow.Worker` is the original background refresh worker. It still works locally (refreshing every registered UPRN every `WorkerSettings:RefreshIntervalHours`), but it is **not deployed**. Do not reintroduce it as the primary refresh mechanism while hosting on F1.

---

## Database Tables

### BinCollections

Stores:

- Recycling
- Food Waste
- Refuse
- AHP (Nappy Waste)

Including:

- LastCollection
- NextCollection
- LastUpdated (UTC)

---

### RegisteredUprns

Stores the UPRNs HomeFlow is allowed to refresh from the council API.

Example:

```text
YOUR_UPRN
```

---

### Logs

Stores application events written through `ILogService`.

- Entries below `LogSettings:MinimumLevel` are not written.
- Entries older than `LogSettings:RetentionDays` are deleted automatically.

---

## Dashboard

The dashboard:

- Calls the API
- Does not directly access the database
- Uses DTO → Mapper → ViewModel transformation

Flow:

```text
API
 ↓
BinCollectionDto
 ↓
DashboardBinMapper
 ↓
DashboardBinViewModel
 ↓
Razor Page
```

---

## Deployment

The Azure DevOps pipeline (`azure-pipelines.yml`) runs on pushes to `development` and:

1. Restores the solution
2. Publishes `HomeFlow.Web.UI` with ReadyToRun for `win-x86` (F1 only runs 32-bit; ReadyToRun shortens cold starts)
3. Zips the output and publishes it as the `drop` artifact

The worker is not included in the package.

### Deployment Checklist

#### Database

- Create database
- Apply migrations
- Add UPRN records to `RegisteredUprns`

#### App Service

- Configure the connection string
- Set `ApiSettings__BaseUrl` to the site's own URL
- Make sure no old `HomeFlowWorker` WebJob is still listed under WebJobs
- Verify Swagger loads
- Verify `GET /api/bins/{uprn}/dashboard` returns data

#### Dashboard

- Verify data loads from the API
- Verify latest collection dates display correctly

---

## Current Project Status

### Completed

- Bin Collection API
- On-demand Bin Refresh
- Database Logging (level filter and retention)
- Dashboard UI
- DTO Mapping
- View Models
- Configuration Management
- UPRN Registration
- Deployment Pipeline

### In Progress

- Pico Dashboard

### Planned

- Weather Integration
- Microsoft Graph Integration
- Calendar Integration
- Home Assistant Integration
- VPS Deployment

---

## Intended Hosting Platform

### Current

```text
Azure App Service (F1, Windows, 32-bit)
SQL Server
.NET 10
```

### Development

```text
Windows 11
SQL Server LocalDB
.NET 10
```

### Target Deployment

```text
Hetzner VPS
Ubuntu
Docker
SQL Server or PostgreSQL
```

---

## Future Features

### Pico Dashboard

The Pico will consume:

```http
GET /api/bins/{uprn}/dashboard
```

and render collection information locally.

### Planned Integrations

- Weather
- Microsoft Graph
- Calendar Events
- Home Assistant
- Energy Monitoring

---

## Notes

- HomeFlow uses .NET 10 throughout the solution.
- Bin collection refreshing is handled on demand by the API (`IBinRefreshService`); the worker is for local use only.
- The Web UI never calls the South Gloucestershire API directly.
- The API is the single source of truth for all consumer applications.
- All UPRNs refreshed by the system are controlled through the `RegisteredUprns` database table.

---

## Status Summary

### Complete

- [x] Bin Collection API
- [x] Dashboard UI
- [x] On-demand Bin Refresh
- [x] Database Logging
- [x] Configuration Management
- [x] DTO / Mapper / ViewModel Architecture
- [x] Registered UPRN Management
- [x] Deployment Pipeline

### Next

- [ ] Pico Integration
- [ ] Test Project (`HomeFlow.Tests`)
- [ ] VPS Hosting
- [ ] Evaluate PostgreSQL Migration
- [ ] Additional Information Services
