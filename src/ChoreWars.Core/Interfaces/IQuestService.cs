using ChoreWars.Core.Commands;
using ChoreWars.Core.Queries;
using ChoreWars.Core.Results;

namespace ChoreWars.Core.Interfaces;

public interface IQuestService
{
    Task<AppResult<QuestCompletionDto>> ClaimQuestAsync(ClaimQuestCommand command);
    Task<AppResult<QuestCompletionDto>> CompleteQuestAsync(CompleteQuestCommand command);
    Task<AppResult<QuestCompletionDto>> VerifyQuestAsync(VerifyQuestCommand command);
    Task<AppResult<QuestCompletionDto>> UnclaimQuestAsync(UnclaimQuestCommand command);
    Task<AppResult<List<QuestDto>>> GetAvailableQuestsAsync(GetAvailableQuestsQuery query);
    Task<AppResult<List<QuestDto>>> GetMyActiveQuestsAsync(GetMyActiveQuestsQuery query);
    Task<AppResult<List<QuestDto>>> ImportQuestsAsync(ImportQuestsCommand command);
}

// DTOs
public class QuestDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int XPReward { get; set; }
    public int GoldReward { get; set; }
    public string Difficulty { get; set; } = string.Empty;
    public string QuestType { get; set; } = string.Empty;
    public int StrengthBonus { get; set; }
    public int IntelligenceBonus { get; set; }
    public int ConstitutionBonus { get; set; }
    public bool IsClaimed { get; set; }
    public string? ClaimedByUsername { get; set; }
    public Guid? CompletionId { get; set; }
    public string? CompletionStatus { get; set; }
}

public class QuestCompletionDto
{
    public Guid Id { get; set; }
    public Guid QuestId { get; set; }
    public string QuestTitle { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int XPEarned { get; set; }
    public int GoldEarned { get; set; }
    public int StrengthGained { get; set; }
    public int IntelligenceGained { get; set; }
    public int ConstitutionGained { get; set; }
    public DateTime ClaimedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
}
