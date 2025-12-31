using ChoreWars.Core.Commands;
using ChoreWars.Core.Queries;
using ChoreWars.Core.Results;

namespace ChoreWars.Core.Interfaces;

public interface IProgressionService
{
    Task<AppResult<UserProgressDto>> AwardXPAsync(AwardXPCommand command);
    Task<AppResult<UserProgressDto>> AwardGoldAsync(AwardGoldCommand command);
    Task<AppResult<UserProgressDto>> AwardStatsAsync(AwardStatsCommand command);
    Task<AppResult<LevelUpDto?>> CheckLevelUpAsync(CheckLevelUpCommand command);
    Task<AppResult<UserProgressDto>> GetUserProgressAsync(GetUserProgressQuery query);
    Task<AppResult<UserStatsDto>> GetUserStatsAsync(GetUserStatsQuery query);
}

// DTOs
public class UserProgressDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int CurrentLevel { get; set; }
    public int CurrentXP { get; set; }
    public int XPToNextLevel { get; set; }
    public int TotalGold { get; set; }
    public int Strength { get; set; }
    public int Intelligence { get; set; }
    public int Constitution { get; set; }
    public bool LeveledUp { get; set; }
}

public class UserStatsDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Strength { get; set; }
    public int Intelligence { get; set; }
    public int Constitution { get; set; }
    public int CurrentLevel { get; set; }
}

public class LevelUpDto
{
    public int OldLevel { get; set; }
    public int NewLevel { get; set; }
    public string Message { get; set; } = string.Empty;
}
