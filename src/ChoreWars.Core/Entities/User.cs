namespace ChoreWars.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public required string DisplayName { get; set; }
    public int CurrentLevel { get; set; } = 1;
    public int CurrentXP { get; set; } = 0;
    public int XPToNextLevel { get; set; } = 100;
    public int TotalGold { get; set; } = 0;
    public string? AvatarUrl { get; set; }
    public Guid PartyId { get; set; }

    // Stat Attributes (RPG Stats)
    public int Strength { get; set; } = 0;
    public int Intelligence { get; set; } = 0;
    public int Constitution { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Party? Party { get; set; }
    public ICollection<QuestCompletion> QuestCompletions { get; set; } = new List<QuestCompletion>();
    public ICollection<RewardPurchase> RewardPurchases { get; set; } = new List<RewardPurchase>();
    public ICollection<LootDrop> LootDrops { get; set; } = new List<LootDrop>();
}
