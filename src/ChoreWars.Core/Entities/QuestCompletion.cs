namespace ChoreWars.Core.Entities;

public class QuestCompletion
{
    public Guid Id { get; set; }
    public Guid QuestId { get; set; }
    public Guid UserId { get; set; }
    public DateTime ClaimedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedByDMId { get; set; }
    public CompletionStatus Status { get; set; } = CompletionStatus.Claimed;
    public int XPEarned { get; set; }
    public int GoldEarned { get; set; }

    // Stat Gains (snapshot of what was earned from this quest)
    public int StrengthGained { get; set; } = 0;
    public int IntelligenceGained { get; set; } = 0;
    public int ConstitutionGained { get; set; } = 0;

    // Navigation properties
    public Quest? Quest { get; set; }
    public User? User { get; set; }
    public LootDrop? LootDrop { get; set; }
}
