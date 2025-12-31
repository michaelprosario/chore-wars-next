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
