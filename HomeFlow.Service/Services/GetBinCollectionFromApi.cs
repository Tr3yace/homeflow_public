using HomeFlow.Data.Models;
using HomeFlow.Data.Settings;
using HomeFlow.Service.Helpers;
using HomeFlow.Service.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HomeFlow.Service.Services
{
    public class GetBinCollectionFromApi : IGetBinCollectionFromApi
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly ILogService _logService;

        public GetBinCollectionFromApi(
            ApplicationDbContext context,
            HttpClient httpClient,
            ILogService logService)
        {
            _context = context;
            _httpClient = httpClient;
            _logService = logService;
        }

        public async Task<bool> FetchAndStoreBinCollectionAsync(string uprn)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(uprn) || !Regex.IsMatch(uprn, @"^\d{6,10}$"))
                {
                    await _logService.WriteAsync(
                        "Warning",
                        $"Invalid UPRN supplied: {uprn}",
                        source: nameof(GetBinCollectionFromApi));

                    return false;
                }

                var baseUrl = Urls.CouncilApiBaseUrl?.TrimEnd('/') ?? string.Empty;
                var url = $"{baseUrl}?uprn={Uri.EscapeDataString(uprn)}";

                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    await _logService.WriteAsync(
                        "Warning",
                        $"Council API returned {response.StatusCode} for UPRN {uprn}",
                        source: nameof(GetBinCollectionFromApi));

                    return false;
                }

                var raw = await response.Content.ReadAsStringAsync();

                await _logService.WriteAsync(
                    "Debug",
                    $"Council API response received for UPRN {uprn}",
                    source: nameof(GetBinCollectionFromApi));

                if (string.IsNullOrWhiteSpace(raw))
                {
                    await _logService.WriteAsync(
                        "Information",
                        $"Empty response received for UPRN {uprn}",
                        source: nameof(GetBinCollectionFromApi));

                    return false;
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                List<CouncilBinItem>? data = null;

                using (var doc = JsonDocument.Parse(raw))
                {
                    var root = doc.RootElement;

                    // OData wrapper: { "@odata.context": "...", "value": [ ... ] }
                    if (root.ValueKind == JsonValueKind.Object &&
                        root.TryGetProperty("value", out var valueElem) &&
                        valueElem.ValueKind == JsonValueKind.Array)
                    {
                        data = JsonSerializer.Deserialize<List<CouncilBinItem>>(
                            valueElem.GetRawText(),
                            options);
                    }
                    else if (root.ValueKind == JsonValueKind.Array)
                    {
                        // fallback: root is the array itself
                        data = JsonSerializer.Deserialize<List<CouncilBinItem>>(
                            raw,
                            options);
                    }
                    else
                    {
                        await _logService.WriteAsync(
                            "Warning",
                            $"Unexpected JSON structure returned for UPRN {uprn}",
                            source: nameof(GetBinCollectionFromApi));

                        return false;
                    }
                }

                if (data == null || data.Count == 0)
                {
                    await _logService.WriteAsync(
                        "Information",
                        $"No bin collection data found for UPRN {uprn}",
                        source: nameof(GetBinCollectionFromApi));

                    return false;
                }

                // Save api data to database
                // Single timestamp for this refresh
                var refreshTime = DateTime.UtcNow;

                // Ignore records that don't have a valid next collection date
                var entities = data
                    .Where(item => item.HsoNextCollection.HasValue)
                    .Select(item => new BinCollection
                    {
                        UPRN = uprn,
                        ServiceName = item.HsoServiceName?.Trim(),
                        Round = item.HsoRound?.Trim(),
                        RoundGroup = item.HsoRoundGroup?.Trim(),
                        ScheduleDescription = item.HsoScheduleDescription?.Trim(),
                        LastCollection = item.HsoLastCollection,
                        NextCollection = item.HsoNextCollection,
                        MissedCollectionAllowed = item.HsoMissedCollectionAllowed,
                        LastUpdated = refreshTime
                    })
                    .ToList();

                // Add sanitied json to logs
                await _logService.WriteAsync(
                    "Information",
                    $"Saving entities: {JsonSerializer.Serialize(
                        entities.Select(x => new
                        {
                            x.ServiceName,
                            x.NextCollection,
                            x.LastUpdated
                        }))}",
                    source: nameof(GetBinCollectionFromApi));

                var existing = await _context.BinCollections
                    .Where(x => x.UPRN == uprn)
                    .ToListAsync();

                _context.BinCollections.RemoveRange(existing);

                await _context.BinCollections.AddRangeAsync(entities);

                await _context.SaveChangesAsync();

                await _logService.WriteAsync(
                    "Information",
                    $"Successfully ingested {entities.Count} bin records for UPRN {uprn}",
                    source: nameof(GetBinCollectionFromApi));

                return true;
            }
            catch (Exception ex)
            {
                // LogService shares this DbContext: drop any pending bin changes from a
                // failed save so the log write does not retry them and fail as well
                _context.ChangeTracker.Clear();

                await _logService.WriteAsync(
                    "Error",
                    $"Error ingesting bin data for UPRN {uprn}",
                    ex,
                    nameof(GetBinCollectionFromApi));

                return false;
            }
        }
    }
}