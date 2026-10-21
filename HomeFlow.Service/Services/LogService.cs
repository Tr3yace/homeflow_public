using HomeFlow.Data.Models;
using HomeFlow.Data.Settings;
using HomeFlow.Service.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HomeFlow.Service.Services
{
    public class LogService : ILogService
    {
        // Retention clean-up runs at most once per interval per process. There is no
        // background timer (F1 has no Always On), so it piggybacks on log writes.
        private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24);
        private static DateTime _lastCleanup = DateTime.MinValue;

        private readonly ApplicationDbContext _context;
        private readonly LogSettings _settings;

        public LogService(ApplicationDbContext context, IOptions<LogSettings> settings)
        {
            _context = context;
            _settings = settings.Value;
        }

        public async Task WriteAsync(string level, string message, Exception ex = null, string source = null)
        {
            if (!IsEnabled(level))
                return;

            var entry = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                Message = message,
                Exception = ex?.Message,
                Source = source
            };

            _context.Logs.Add(entry);
            await _context.SaveChangesAsync();

            await CleanupIfDueAsync();
        }

        private bool IsEnabled(string level)
        {
            // Unknown levels are always written rather than silently dropped
            if (!Enum.TryParse<LogLevel>(level, true, out var entryLevel) ||
                !Enum.TryParse<LogLevel>(_settings.MinimumLevel, true, out var minimumLevel))
                return true;

            return entryLevel >= minimumLevel;
        }

        private async Task CleanupIfDueAsync()
        {
            if (_settings.RetentionDays <= 0 || DateTime.UtcNow - _lastCleanup < CleanupInterval)
                return;

            _lastCleanup = DateTime.UtcNow;

            try
            {
                var cutoff = DateTime.UtcNow.AddDays(-_settings.RetentionDays);

                await _context.Logs
                    .Where(x => x.Timestamp < cutoff)
                    .ExecuteDeleteAsync();
            }
            catch
            {
                // Clean-up is best effort; it must never break the caller's log write
            }
        }
    }
}
