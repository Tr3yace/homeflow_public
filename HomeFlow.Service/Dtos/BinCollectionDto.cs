namespace HomeFlow.Data.Models.DTO
{
    public class BinCollectionDto
    {
        public string? ServiceName { get; set; }
        public string? Round { get; set; }
        public string? RoundGroup { get; set; }
        public string? ScheduleDescription { get; set; }
        public DateTimeOffset? LastCollection { get; set; }
        public DateTimeOffset? NextCollection { get; set; }
        public bool MissedCollectionAllowed { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
