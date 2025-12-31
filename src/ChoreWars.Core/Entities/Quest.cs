namespace ChoreWars.Core.Entities;

public class Quest
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int XPReward { get; set; }
    public int GoldReward { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public QuestType QuestType { get; set; }

    // Stat Bonuses (Optional - can be 0 if quest doesn't grant stats)
    public int StrengthBonus { get; set; } = 0;
    public int IntelligenceBonus { get; set; } = 0;
    public int ConstitutionBonus { get; set; } = 0;

    public Guid PartyId { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedByDMId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Party? Party { get; set; }
    public ICollection<QuestCompletion> Completions { get; set; } = new List<QuestCompletion>();
}
