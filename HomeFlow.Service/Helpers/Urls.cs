namespace HomeFlow.Service.Helpers
{
    public static class Urls
    {
        public const string CouncilApiBaseUrl = "https://api.southglos.gov.uk/wastecomp/GetCollectionDetails";

        public static string DashboardEndpoint(
            string baseUrl,
            string uprn)
        {
            return $"{baseUrl.TrimEnd('/')}/api/bins/{uprn}/dashboard";
        }
    }
}