namespace ChoreWars.Core.Commands;

public class ClaimQuestCommand
{
    public required Guid QuestId { get; set; }
    public required Guid UserId { get; set; }
}

public class CompleteQuestCommand
{
    public required Guid QuestCompletionId { get; set; }
    public required Guid UserId { get; set; }
}

public class VerifyQuestCommand
{
    public required Guid QuestCompletionId { get; set; }
    public required Guid DMId { get; set; }
    public required bool IsApproved { get; set; }
}

public class UnclaimQuestCommand
{
    public required Guid QuestCompletionId { get; set; }
    public required Guid UserId { get; set; }
}

public class ImportQuestsCommand
{
    public required List<QuestImportDto> Quests { get; set; }
    public required Guid PartyId { get; set; }
    public required Guid DMId { get; set; }
}

public class QuestImportDto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required int XPReward { get; set; }
    public required int GoldReward { get; set; }
    public required string Difficulty { get; set; }
    public string? QuestType { get; set; }
    public int StrengthBonus { get; set; } = 0;
    public int IntelligenceBonus { get; set; } = 0;
    public int ConstitutionBonus { get; set; } = 0;
}
