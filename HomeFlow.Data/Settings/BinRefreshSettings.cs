namespace HomeFlow.Data.Settings
{
    public class BinRefreshSettings
    {
        // Stored data older than this is refreshed from the council API on the next request
        public int MaxAgeHours { get; set; } = 24;

        // After a failed refresh, wait this long before trying the council API again
        public int FailureCooldownMinutes { get; set; } = 15;
    }
}
