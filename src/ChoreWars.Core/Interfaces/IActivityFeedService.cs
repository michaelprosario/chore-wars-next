using ChoreWars.Core.Entities;
using ChoreWars.Core.Results;

namespace ChoreWars.Core.Interfaces;

public interface IActivityFeedService
{
    Task<AppResult<ActivityFeedItem>> CreateQuestCompletedActivityAsync(
        Guid userId,
        Guid partyId,
        string questTitle,
        int xpEarned,
        int goldEarned,
        int strengthGained,
        int intelligenceGained,
        int constitutionGained);

    Task<AppResult<ActivityFeedItem>> CreateLevelUpActivityAsync(
        Guid userId,
        Guid partyId,
        int newLevel);

    Task<AppResult<ActivityFeedItem>> CreateLootFoundActivityAsync(
        Guid userId,
        Guid partyId,
        string lootName);

    Task<AppResult<ActivityFeedItem>> CreateStatMilestoneActivityAsync(
        Guid userId,
        Guid partyId,
        string statName,
        int statValue,
        string milestoneTitle);

    Task<AppResult<List<ActivityFeedItem>>> GetRecentActivityForPartyAsync(
        Guid partyId,
        int count = 50);
}
