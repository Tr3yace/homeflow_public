# HomeFlow

Household information platform: aggregates home-related data (currently South Gloucestershire bin collections) behind a central REST API, shown on a Razor Pages dashboard and, later, a Pico display.

## Stack

- .NET 10, ASP.NET Core, Razor Pages, Entity Framework Core, SQL Server (LocalDB in dev)
- Solution file is `HomeFlow.slnx` (needs .NET 9+ SDK / recent Visual Studio)

## Solution structure

- `HomeFlow.Data`: entities, DTOs, settings classes, `ApplicationDbContext`
- `HomeFlow.Service`: services, controllers, helpers, extensions
- `HomeFlow.Web.UI`: Razor Pages, view models, mappers; also hosts the API and Swagger
- `HomeFlow.Worker`: legacy background refresh worker, local use only (see "Bin refresh" below)

Reference direction: Web.UI -> Service -> Data. Never reverse it.

## Architecture rules

- The Web UI never touches the database or the council API directly. All consumers (dashboard, Pico) go through the API.
- Dashboard data flow: API -> `BinCollectionDto` -> `DashboardBinMapper` -> `DashboardBinViewModel` -> Razor Page.
- The Pico consumes `GET /api/bins/{uprn}/dashboard`.
- Monitored UPRNs are controlled by the `RegisteredUprns` table. Only registered UPRNs may trigger a council API call.

## Bin refresh (important)

Refresh happens **on demand in the API**, not via the worker. `IBinRefreshService.EnsureFreshAsync(uprn)` (`HomeFlow.Service/Services/BinRefreshService.cs`) is called by `GET api/bins/{uprn}/dashboard` and `GET api/bins/{uprn}/next` before reading data. It:

- refreshes only if the newest `LastUpdated` for the UPRN is older than `BinRefreshSettings.MaxAgeHours` (default 24)
- only calls the council for UPRNs in `RegisteredUprns`
- serialises concurrent refreshes per UPRN (static per-UPRN `SemaphoreSlim`, re-checks staleness after acquiring it)
- backs off after a failure for `FailureCooldownMinutes` (default 15)
- never throws; callers get the stored data

The council `HttpClient` has a 10s timeout because the refresh runs inside a request.

Why: hosting is an Azure **F1** web app, which has no Always On, so WebJobs and background timers are unreliable. Do not reintroduce a background timer or WebJob as the primary refresh mechanism. `HomeFlow.Worker` is kept for local use only and is not deployed.

## Logging

- Logging goes through `ILogService.WriteAsync(level, message, ex, source)` into the `Logs` table.
- Levels are strings matching `Microsoft.Extensions.Logging.LogLevel` names (`Debug`, `Information`, `Warning`, `Error`). Entries below `LogSettings.MinimumLevel` (default `Information`) are not written; unknown levels are always written.
- Retention: entries older than `LogSettings.RetentionDays` (default 30) are deleted. There is no timer: `LogService` runs the delete at most once every 24 hours per process, piggybacking on a log write.
- `LogService` shares the scoped `ApplicationDbContext` with other services. After a failed `SaveChangesAsync`, call `ChangeTracker.Clear()` before logging, otherwise the log write retries the failed changes and fails too.

## Conventions

- `LastUpdated` values are stored in UTC.
- The repo is public. Never commit a real UPRN (it identifies a home address), keys, or connection strings. `DashboardSettings:DefaultUprn` and `ApiSettings:AdminKey` come from user secrets locally and App Service settings in Azure; use `YOUR_UPRN` in docs.
- `POST api/bins/{uprn}/fetchfromapi` requires the `X-Api-Key` header matching `ApiSettings:AdminKey` (endpoint disabled when unset) and a registered UPRN.
- Settings classes live in `HomeFlow.Data.Settings` and are bound with `IOptions<T>` (`DashboardSettings`, `ApiSettings`, `BinRefreshSettings`, `LogSettings`, `WorkerSettings`).
- Keep the casing of project and folder names exactly `HomeFlow.*`. The target environment (Linux/Docker) is case-sensitive.

## Build and run (local)

```bash
dotnet restore
dotnet ef database update --project HomeFlow.Data
```

Run `HomeFlow.Web.UI` (Swagger at `/swagger`, dashboard at `/Dashboard`). At least one row must exist in `RegisteredUprns`. `ApiSettings:BaseUrl` must point at the running app, because the dashboard page calls the API over HTTP.

## Deployment

- Azure DevOps YAML pipeline (`azure-pipelines.yml`), triggers on the `development` branch, windows-latest agent, .NET 10 SDK.
- Publishes only `HomeFlow.Web.UI` (no WebJob) and zips it as the `drop` artifact; deployment to the Azure App Service (F1 tier, Windows) happens from that artifact.
- Publish uses `--runtime win-x86 --self-contained false -p:PublishReadyToRun=true` to cut cold-start time. F1 only runs 32-bit, so keep `win-x86` while on F1.
- `DotNetCoreCLI@2` publish steps need `modifyOutputPath: false`, otherwise output lands in an extra project-named subfolder.
- In Azure, `ApiSettings:BaseUrl` must be the site's own URL and `RegisteredUprns` must contain the dashboard UPRN.
- Target later: Hetzner VPS, Ubuntu, Docker, SQL Server or PostgreSQL. On a VPS a background refresh becomes reliable again, but keep on-demand refresh as the safety net.

## Known issues / TODO

- Casing: folders and project paths are now `HomeFlow.*`, but the worker's project file is still `HomeFlow.Worker/Homeflow.Worker.csproj` (lower-case f, so its assembly is `Homeflow.Worker.dll`). Rename it before moving to Docker if the worker is ever deployed there.
- No test project yet; add `HomeFlow.Tests` (Service layer first, starting with `BinRefreshService` and `LogService`).
- Planned: Pico dashboard, weather, Microsoft Graph, calendar, Home Assistant, VPS deployment.
