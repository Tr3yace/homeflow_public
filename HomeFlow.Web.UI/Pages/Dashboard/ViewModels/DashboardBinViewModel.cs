namespace HomeFlow.Web.UI.Pages.Dashboard.ViewModels
{
    public class DashboardBinViewModel
    {
        public string ServiceName { get; set; } = string.Empty;

        public DateTimeOffset? NextCollection { get; set; }

        public string CollectionStatus { get; set; } = string.Empty;

        public string CssClass { get; set; } = string.Empty;

        public DateTime LastUpdated { get; set; }

        public string DayOfWeek =>
            NextCollection?.ToString("dddd") ?? string.Empty;

        public string NextCollectionDisplay =>
            NextCollection?.ToString("dd MMM yyyy") ?? "Unknown";

        public string LastUpdatedDisplay =>
            LastUpdated.ToLocalTime().ToString("dd MMM yyyy HH:mm");
    }
}