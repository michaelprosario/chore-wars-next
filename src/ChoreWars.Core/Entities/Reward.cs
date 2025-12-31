namespace ChoreWars.Core.Entities;

public class Reward
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int GoldCost { get; set; }
    public Guid PartyId { get; set; }
    public bool IsAvailable { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Party? Party { get; set; }
    public ICollection<RewardPurchase> Purchases { get; set; } = new List<RewardPurchase>();
}
