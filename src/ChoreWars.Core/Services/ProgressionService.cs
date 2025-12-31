using ChoreWars.Core.Commands;
using ChoreWars.Core.Interfaces;
using ChoreWars.Core.Queries;
using ChoreWars.Core.Results;

namespace ChoreWars.Core.Services;

public class ProgressionService : IProgressionService
{
    private readonly IUserRepository _userRepository;

    public ProgressionService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AppResult<UserProgressDto>> AwardXPAsync(AwardXPCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user == null)
        {
            return AppResult<UserProgressDto>.Failure("User not found");
        }

        user.CurrentXP += command.XPAmount;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        var dto = MapToProgressDto(user);
        return AppResult<UserProgressDto>.Success(dto, $"Awarded {command.XPAmount} XP");
    }

    public async Task<AppResult<UserProgressDto>> AwardGoldAsync(AwardGoldCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user == null)
        {
            return AppResult<UserProgressDto>.Failure("User not found");
        }

        user.TotalGold += command.GoldAmount;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        var dto = MapToProgressDto(user);
        return AppResult<UserProgressDto>.Success(dto, $"Awarded {command.GoldAmount} gold");
    }

    public async Task<AppResult<UserProgressDto>> AwardStatsAsync(AwardStatsCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user == null)
        {
            return AppResult<UserProgressDto>.Failure("User not found");
        }

        user.Strength += command.StrengthBonus;
        user.Intelligence += command.IntelligenceBonus;
        user.Constitution += command.ConstitutionBonus;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        var dto = MapToProgressDto(user);
        var statsMessage = BuildStatsMessage(command.StrengthBonus, command.IntelligenceBonus, command.ConstitutionBonus);
        return AppResult<UserProgressDto>.Success(dto, statsMessage);
    }

    public async Task<AppResult<LevelUpDto?>> CheckLevelUpAsync(CheckLevelUpCommand command)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user == null)
        {
            return AppResult<LevelUpDto?>.Failure("User not found");
        }

        bool leveledUp = false;
        int oldLevel = user.CurrentLevel;

        while (user.CurrentXP >= user.XPToNextLevel)
        {
            user.CurrentXP -= user.XPToNextLevel;
            user.CurrentLevel++;
            user.XPToNextLevel = CalculateXPForNextLevel(user.CurrentLevel);
            leveledUp = true;
        }

        if (leveledUp)
        {
            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            var levelUpDto = new LevelUpDto
            {
                OldLevel = oldLevel,
                NewLevel = user.CurrentLevel,
                Message = $"LEVEL UP! You are now level {user.CurrentLevel}!"
            };

            return AppResult<LevelUpDto?>.Success(levelUpDto, "Level up!");
        }

        return AppResult<LevelUpDto?>.Success(null, "No level up");
    }

    public async Task<AppResult<UserProgressDto>> GetUserProgressAsync(GetUserProgressQuery query)
    {
        var user = await _userRepository.GetByIdAsync(query.UserId);
        if (user == null)
        {
            return AppResult<UserProgressDto>.Failure("User not found");
        }

        var dto = MapToProgressDto(user);
        return AppResult<UserProgressDto>.Success(dto);
    }

    public async Task<AppResult<UserStatsDto>> GetUserStatsAsync(GetUserStatsQuery query)
    {
        var user = await _userRepository.GetUserWithStatsAsync(query.UserId);
        if (user == null)
        {
            return AppResult<UserStatsDto>.Failure("User not found");
        }

        var dto = new UserStatsDto
        {
            UserId = user.Id,
            Username = user.Username,
            Strength = user.Strength,
            Intelligence = user.Intelligence,
            Constitution = user.Constitution,
            CurrentLevel = user.CurrentLevel
        };

        return AppResult<UserStatsDto>.Success(dto);
    }

    private UserProgressDto MapToProgressDto(Entities.User user)
    {
        return new UserProgressDto
        {
            UserId = user.Id,
            Username = user.Username,
            CurrentLevel = user.CurrentLevel,
            CurrentXP = user.CurrentXP,
            XPToNextLevel = user.XPToNextLevel,
            TotalGold = user.TotalGold,
            Strength = user.Strength,
            Intelligence = user.Intelligence,
            Constitution = user.Constitution,
            LeveledUp = false
        };
    }

    private int CalculateXPForNextLevel(int currentLevel)
    {
        // Exponential scaling: Base 100 XP, multiply by 1.5 for each level
        return (int)(100 * Math.Pow(1.5, currentLevel - 1));
    }

    private string BuildStatsMessage(int str, int intel, int con)
    {
        var messages = new List<string>();
        if (str > 0) messages.Add($"+{str} Strength");
        if (intel > 0) messages.Add($"+{intel} Intelligence");
        if (con > 0) messages.Add($"+{con} Constitution");

        return messages.Count > 0 ? $"Awarded {string.Join(", ", messages)}" : "No stats awarded";
    }
}
