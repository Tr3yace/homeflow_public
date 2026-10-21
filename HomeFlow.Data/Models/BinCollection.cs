public class BinCollection
{
    public Guid Id { get; set; }
    public Guid VirtualServiceInformationId { get; set; }
    public string? UPRN { get; set; }
    public string? ServiceName { get; set; }
    public string? RoundLeg { get; set; }
    public string? Round { get; set; } = null;  
    public string? RoundGroup { get; set; }
    public string? ScheduleDescription { get; set; }
    public string? StateSource { get; set; }
    public string? StateName { get; set; }
    public string? Reason { get; set; }
    public string? ResolutionName { get; set; }
    public string? ResolutionSource { get; set; }
    public bool MissedCollectionAllowed { get; set; }
    public DateTimeOffset? LastCollection { get; set; }
    public DateTimeOffset? LastCollectionCompleted { get; set; }
    public DateTimeOffset? NextCollection { get; set; }
    public DateTimeOffset? SubscriptionEndDate { get; set; }
    public string? Name { get; set; }
    public int ServiceId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
}
