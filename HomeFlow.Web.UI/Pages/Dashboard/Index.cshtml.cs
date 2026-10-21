using HomeFlow.Data.Models.DTO;
using HomeFlow.Data.Settings;
using HomeFlow.Service.Helpers;
using HomeFlow.Web.UI.Mappers;
using HomeFlow.Web.UI.Pages.Dashboard.ViewModels;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace HomeFlow.Web.UI.Pages.Dashboard
{
    public class IndexModel : PageModel
    {
        private readonly HttpClient _httpClient;
        private readonly DashboardSettings _dashboardSettings;
        private readonly ApiSettings _apiSettings;

        public List<DashboardBinViewModel> Bins { get; set; } = [];

        public IndexModel(
            HttpClient httpClient,
            IOptions<DashboardSettings> dashboardSettings,
            IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _dashboardSettings = dashboardSettings.Value;
            _apiSettings = apiSettings.Value;
        }

        public async Task OnGetAsync()
        {
            var uprn = _dashboardSettings.DefaultUprn;

            // Not configured (it is kept out of source control): show an empty dashboard
            if (string.IsNullOrWhiteSpace(uprn))
                return;

            var binsFromApi =
                await _httpClient.GetFromJsonAsync<List<BinCollectionDto>>
                (
                    Urls.DashboardEndpoint(
                        _apiSettings.BaseUrl,
                        uprn)
                );

            if (binsFromApi is null)
                return;

            Bins = binsFromApi
                .Select(DashboardBinMapper.Map)
                .OfType<DashboardBinViewModel>()
                .ToList();
        }
    }
}
