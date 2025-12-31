namespace ChoreWars.Core.Entities;

public class ActivityFeedItem
{
    public Guid Id { get; set; }
    public Guid PartyId { get; set; }
    public Guid UserId { get; set; }
    public ActivityType ActivityType { get; set; }
    public required string Message { get; set; } // Flavor text
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? Metadata { get; set; } // JSON - stores additional context like which stat increased

    // Navigation properties
    public Party? Party { get; set; }
    public User? User { get; set; }
}
