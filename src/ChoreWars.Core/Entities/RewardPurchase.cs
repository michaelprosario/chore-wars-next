namespace ChoreWars.Core.Entities;

public class RewardPurchase
{
    public Guid Id { get; set; }
    public Guid RewardId { get; set; }
    public Guid UserId { get; set; }
    public int GoldSpent { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FulfilledAt { get; set; }
    public Guid? FulfilledByDMId { get; set; }
    public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
    public string? Memo { get; set; }

    // Navigation properties
    public Reward? Reward { get; set; }
    public User? User { get; set; }
}
