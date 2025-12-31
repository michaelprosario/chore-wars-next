using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Core.Results;
using System.Text.Json;

namespace ChoreWars.Core.Services;

public class ActivityFeedService : IActivityFeedService
{
    private readonly IRepository<ActivityFeedItem> _activityFeedRepository;
    private readonly IUserRepository _userRepository;

    public ActivityFeedService(
        IRepository<ActivityFeedItem> activityFeedRepository,
        IUserRepository userRepository)
    {
        _activityFeedRepository = activityFeedRepository;
        _userRepository = userRepository;
    }

    public async Task<AppResult<ActivityFeedItem>> CreateQuestCompletedActivityAsync(
        Guid userId,
        Guid partyId,
        string questTitle,
        int xpEarned,
        int goldEarned,
        int strengthGained,
        int intelligenceGained,
        int constitutionGained)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return AppResult<ActivityFeedItem>.Failure("User not found");
        }

        // Build stat gains text
        var statParts = new List<string>();
        if (strengthGained > 0) statParts.Add($"💪+{strengthGained} STR");
        if (intelligenceGained > 0) statParts.Add($"🧠+{intelligenceGained} INT");
        if (constitutionGained > 0) statParts.Add($"❤️+{constitutionGained} CON");

        var statText = statParts.Count > 0 ? $", {string.Join(", ", statParts)}" : "";

        var message = $"{user.DisplayName} vanquished {questTitle.ToUpper()}! " +
                      $"(+{xpEarned} XP, +{goldEarned} Gold{statText})";

        var metadata = JsonSerializer.Serialize(new
        {
            QuestTitle = questTitle,
            XPEarned = xpEarned,
            GoldEarned = goldEarned,
            StrengthGained = strengthGained,
            IntelligenceGained = intelligenceGained,
            ConstitutionGained = constitutionGained
        });

        var activityItem = new ActivityFeedItem
        {
            Id = Guid.NewGuid(),
            PartyId = partyId,
            UserId = userId,
            ActivityType = ActivityType.QuestCompleted,
            Message = message,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };

        await _activityFeedRepository.AddAsync(activityItem);
        await _activityFeedRepository.SaveChangesAsync();

        return AppResult<ActivityFeedItem>.Success(activityItem);
    }

    public async Task<AppResult<ActivityFeedItem>> CreateLevelUpActivityAsync(
        Guid userId,
        Guid partyId,
        int newLevel)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return AppResult<ActivityFeedItem>.Failure("User not found");
        }

        var message = $"🎉 {user.DisplayName} reached Level {newLevel}!";

        var metadata = JsonSerializer.Serialize(new
        {
            NewLevel = newLevel
        });

        var activityItem = new ActivityFeedItem
        {
            Id = Guid.NewGuid(),
            PartyId = partyId,
            UserId = userId,
            ActivityType = ActivityType.LevelUp,
            Message = message,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };

        await _activityFeedRepository.AddAsync(activityItem);
        await _activityFeedRepository.SaveChangesAsync();

        return AppResult<ActivityFeedItem>.Success(activityItem);
    }

    public async Task<AppResult<ActivityFeedItem>> CreateLootFoundActivityAsync(
        Guid userId,
        Guid partyId,
        string lootName)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return AppResult<ActivityFeedItem>.Failure("User not found");
        }

        var message = $"🎁 {user.DisplayName} found legendary loot: {lootName}!";

        var metadata = JsonSerializer.Serialize(new
        {
            LootName = lootName
        });

        var activityItem = new ActivityFeedItem
        {
            Id = Guid.NewGuid(),
            PartyId = partyId,
            UserId = userId,
            ActivityType = ActivityType.LootFound,
            Message = message,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };

        await _activityFeedRepository.AddAsync(activityItem);
        await _activityFeedRepository.SaveChangesAsync();

        return AppResult<ActivityFeedItem>.Success(activityItem);
    }

    public async Task<AppResult<ActivityFeedItem>> CreateStatMilestoneActivityAsync(
        Guid userId,
        Guid partyId,
        string statName,
        int statValue,
        string milestoneTitle)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return AppResult<ActivityFeedItem>.Failure("User not found");
        }

        var statEmoji = statName switch
        {
            "Strength" => "💪",
            "Intelligence" => "🧠",
            "Constitution" => "❤️",
            _ => "⭐"
        };

        var message = $"{statEmoji} {user.DisplayName}'s {statName} reached {statValue}! " +
                      $"Earned title: \"{milestoneTitle}\"";

        var metadata = JsonSerializer.Serialize(new
        {
            StatName = statName,
            StatValue = statValue,
            MilestoneTitle = milestoneTitle
        });

        var activityItem = new ActivityFeedItem
        {
            Id = Guid.NewGuid(),
            PartyId = partyId,
            UserId = userId,
            ActivityType = ActivityType.StatMilestone,
            Message = message,
            Metadata = metadata,
            CreatedAt = DateTime.UtcNow
        };

        await _activityFeedRepository.AddAsync(activityItem);
        await _activityFeedRepository.SaveChangesAsync();

        return AppResult<ActivityFeedItem>.Success(activityItem);
    }

    public async Task<AppResult<List<ActivityFeedItem>>> GetRecentActivityForPartyAsync(
        Guid partyId,
        int count = 50)
    {
        var activities = await _activityFeedRepository.GetAllByConditionAsync(
            a => a.PartyId == partyId);

        var sortedActivities = activities
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToList();

        return AppResult<List<ActivityFeedItem>>.Success(sortedActivities);
    }
}
