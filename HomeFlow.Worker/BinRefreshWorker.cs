using HomeFlow.Data.Settings;
using HomeFlow.Service.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HomeFlow.Worker
{
    public class BinRefreshWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly WorkerSettings _workerSettings;

        public BinRefreshWorker(
            IServiceScopeFactory scopeFactory,
            IOptions<WorkerSettings> workerSettings)
        {
            _scopeFactory = scopeFactory;
            _workerSettings = workerSettings.Value;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();

                    var context =
                        scope.ServiceProvider
                            .GetRequiredService<ApplicationDbContext>();

                    var ingestion =
                        scope.ServiceProvider
                            .GetRequiredService<IGetBinCollectionFromApi>();

                    var logService =
                        scope.ServiceProvider
                            .GetRequiredService<ILogService>();

                    await logService.WriteAsync(
                        "Information",
                        "Starting bin refresh cycle.",
                        source: nameof(BinRefreshWorker));

                    var uprns = await context.RegisteredUprns
                        .Select(x => x.Uprn)
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .ToListAsync(stoppingToken);

                    await logService.WriteAsync(
                        "Information",
                        $"Found {uprns.Count} registered UPRN(s).",
                        source: nameof(BinRefreshWorker));

                    foreach (var uprn in uprns)
                    {
                        var success =
                            await ingestion.FetchAndStoreBinCollectionAsync(
                                uprn!);

                        if (!success)
                        {
                            await logService.WriteAsync(
                                "Warning",
                                $"Failed to refresh UPRN {uprn}.",
                                source: nameof(BinRefreshWorker));
                        }
                    }

                    await logService.WriteAsync(
                        "Information",
                        $"Bin refresh cycle complete. Next refresh in {_workerSettings.RefreshIntervalHours} hour(s).",
                        source: nameof(BinRefreshWorker));
                }
                catch (Exception ex)
                {
                    using var scope = _scopeFactory.CreateScope();

                    var logService =
                        scope.ServiceProvider
                            .GetRequiredService<ILogService>();

                    await logService.WriteAsync(
                        "Error",
                        "Error during bin refresh cycle.",
                        ex,
                        nameof(BinRefreshWorker));
                }

                await Task.Delay(
                    TimeSpan.FromHours(
                        _workerSettings.RefreshIntervalHours),
                    stoppingToken);
            }
        }
    }
}
