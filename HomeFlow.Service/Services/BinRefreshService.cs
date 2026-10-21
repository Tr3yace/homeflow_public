using System.Collections.Concurrent;
using HomeFlow.Data.Settings;
using HomeFlow.Service.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace HomeFlow.Service.Services
{
    public class BinRefreshService : IBinRefreshService
    {
        // Shared across requests (the service itself is scoped) so concurrent
        // requests for the same UPRN only trigger one council API call.
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> RefreshLocks = new();

        // Time of the last failed refresh per UPRN, used for the failure cooldown.
        private static readonly ConcurrentDictionary<string, DateTime> LastFailures = new();

        private readonly ApplicationDbContext _context;
        private readonly IGetBinCollectionFromApi _ingestion;
        private readonly ILogService _logService;
        private readonly BinRefreshSettings _settings;

        public BinRefreshService(
            ApplicationDbContext context,
            IGetBinCollectionFromApi ingestion,
            ILogService logService,
            IOptions<BinRefreshSettings> settings)
        {
            _context = context;
            _ingestion = ingestion;
            _logService = logService;
            _settings = settings.Value;
        }

        public async Task EnsureFreshAsync(string uprn, CancellationToken cancellationToken = default)
        {
            try
            {
                if (!await IsStaleAsync(uprn, cancellationToken) || IsInFailureCooldown(uprn))
                    return;

                // Only registered UPRNs may trigger a council API call
                var isRegistered = await _context.RegisteredUprns
                    .AnyAsync(x => x.Uprn == uprn, cancellationToken);

                if (!isRegistered)
                    return;

                var refreshLock = RefreshLocks.GetOrAdd(uprn, _ => new SemaphoreSlim(1, 1));

                await refreshLock.WaitAsync(cancellationToken);

                try
                {
                    // Another request may have refreshed (or failed) while we waited
                    if (!await IsStaleAsync(uprn, cancellationToken) || IsInFailureCooldown(uprn))
                        return;

                    var success = await _ingestion.FetchAndStoreBinCollectionAsync(uprn);

                    if (success)
                    {
                        LastFailures.TryRemove(uprn, out _);
                        return;
                    }

                    LastFailures[uprn] = DateTime.UtcNow;

                    await _logService.WriteAsync(
                        "Warning",
                        $"On-demand refresh failed for UPRN {uprn}. Serving stored data; next attempt in {_settings.FailureCooldownMinutes} minute(s).",
                        source: nameof(BinRefreshService));
                }
                finally
                {
                    refreshLock.Release();
                }
            }
            catch (OperationCanceledException)
            {
                // Request was aborted; nothing to refresh for
            }
            catch (Exception ex)
            {
                LastFailures[uprn] = DateTime.UtcNow;

                // Drop any entities left pending by the failed refresh so they are not
                // re-saved by the log write or by later work in this request
                _context.ChangeTracker.Clear();

                try
                {
                    await _logService.WriteAsync(
                        "Error",
                        $"Unexpected error during on-demand refresh for UPRN {uprn}.",
                        ex,
                        nameof(BinRefreshService));
                }
                catch
                {
                    // Never let a refresh failure break the caller
                }
            }
        }

        private async Task<bool> IsStaleAsync(string uprn, CancellationToken cancellationToken)
        {
            var lastUpdated = await _context.BinCollections
                .Where(x => x.UPRN == uprn)
                .MaxAsync(x => (DateTime?)x.LastUpdated, cancellationToken);

            return lastUpdated is null
                || lastUpdated.Value < DateTime.UtcNow.AddHours(-_settings.MaxAgeHours);
        }

        private bool IsInFailureCooldown(string uprn)
        {
            return LastFailures.TryGetValue(uprn, out var lastFailure)
                && lastFailure > DateTime.UtcNow.AddMinutes(-_settings.FailureCooldownMinutes);
        }
    }
}
