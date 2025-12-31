namespace ChoreWars.Core.Entities;

public class BossBattle
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; }
    public int RequiredTotalXP { get; set; }
    public int CurrentXP { get; set; } = 0;
    public Guid PartyId { get; set; }
    public required string GroupRewardDescription { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public Party? Party { get; set; }
}
