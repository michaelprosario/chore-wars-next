namespace ChoreWars.Core.Entities;

public class LootDrop
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Description { get; set; } // Flavor text
    public Rarity Rarity { get; set; }
    public string? IconUrl { get; set; }
    public Guid UserId { get; set; }
    public Guid QuestCompletionId { get; set; }
    public DateTime FoundAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public User? User { get; set; }
    public QuestCompletion? QuestCompletion { get; set; }
}
