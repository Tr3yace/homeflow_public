using HomeFlow.Data.Settings;
using HomeFlow.Service.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace HomeFlow.Service.Controllers
{
    [ApiController]
    [Route("api/bins")]
    public class BinCollection : ControllerBase
    {
        private readonly IBinCollectionService _service;
        private readonly IGetBinCollectionFromApi _ingestion;
        private readonly IBinRefreshService _refresh;
        private readonly ApiSettings _apiSettings;

        public BinCollection(
            IBinCollectionService service,
            IGetBinCollectionFromApi ingestion,
            IBinRefreshService refresh,
            IOptions<ApiSettings> apiSettings)
        {
            _service = service;
            _ingestion = ingestion;
            _refresh = refresh;
            _apiSettings = apiSettings.Value;
        }

        // GET api/bins/{uprn}/next
        [HttpGet("{uprn}/next")]
        public async Task<IActionResult> GetNext(string uprn)
        {
            if (!IsValidUprn(uprn))
                return BadRequest("Invalid UPRN format.");

            // Refresh from the council API first if the stored data is stale (never throws)
            await _refresh.EnsureFreshAsync(uprn, HttpContext.RequestAborted);

            var result = await _service.GetNextCollectionAsync(uprn);

            return result == null ? NotFound() : Ok(result);
        }

        // POST api/bins/{uprn}/fetchfromapi
        // Manual refresh: requires the admin key and a registered UPRN.
        [HttpPost("{uprn}/fetchfromapi")]
        public async Task<IActionResult> FetchFromApi(
            string uprn,
            [FromHeader(Name = "X-Api-Key")] string? apiKey)
        {
            if (!IsValidAdminKey(apiKey))
                return Unauthorized();

            if (!IsValidUprn(uprn))
                return BadRequest("Invalid UPRN format.");

            if (!await _service.IsRegisteredAsync(uprn))
                return NotFound("UPRN is not registered.");

            var success = await _ingestion.FetchAndStoreBinCollectionAsync(uprn);

            return success ? Ok("Fetching and storing bin collection complete.") : StatusCode(500, "Failed to fetch and store bin collection.");
        }

        // GET api/bins/{uprn}/dashboard
        [HttpGet("{uprn}/dashboard")]
        public async Task<IActionResult> GetDashboard(string uprn)
        {
            if (!IsValidUprn(uprn))
                return BadRequest("Invalid UPRN format.");

            // Refresh from the council API first if the stored data is stale (never throws)
            await _refresh.EnsureFreshAsync(uprn, HttpContext.RequestAborted);

            var result = await _service.GetAllCollectionsAsync(uprn);

            return Ok(result);
        }

        // Used by PICO to query the health of the service. Returns a simple status message and timestamp.
        // GET api/bins/health
        [HttpGet("health")]
        public IActionResult Health()
        {
            return Ok(new { status = "ok", timestamp = DateTime.UtcNow });
        }

        private bool IsValidUprn(string uprn)
        {
            return Regex.IsMatch(uprn, @"^\d{6,10}$");
        }

        private bool IsValidAdminKey(string? apiKey)
        {
            // No key configured means the endpoint is disabled
            if (string.IsNullOrEmpty(_apiSettings.AdminKey) || string.IsNullOrEmpty(apiKey))
                return false;

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(apiKey),
                Encoding.UTF8.GetBytes(_apiSettings.AdminKey));
        }
    }
}
