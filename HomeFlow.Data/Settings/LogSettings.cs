namespace HomeFlow.Data.Settings
{
    public class LogSettings
    {
        // Entries below this level (Debug, Information, Warning, Error) are not written to the Logs table
        public string MinimumLevel { get; set; } = "Information";

        // Entries older than this are deleted; 0 or less keeps them forever
        public int RetentionDays { get; set; } = 30;
    }
}
