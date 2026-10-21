namespace HomeFlow.Data.Settings
{
    public class ApiSettings
    {
        public string BaseUrl { get; set; } = string.Empty;

        // Required in the X-Api-Key header for manual refresh (POST fetchfromapi).
        // Keep it out of source control; when empty the endpoint is disabled.
        public string AdminKey { get; set; } = string.Empty;
    }
}