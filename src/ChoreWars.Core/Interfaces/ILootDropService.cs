using ChoreWars.Core.Entities;
using ChoreWars.Core.Results;

namespace ChoreWars.Core.Interfaces;

public interface ILootDropService
{
    /// <summary>
    /// Attempts to generate a random loot drop (20% chance)
    /// </summary>
    Task<AppResult<LootDrop?>> TryGenerateLootDropAsync(
        Guid userId,
        Guid questCompletionId);

    /// <summary>
    /// Gets all loot drops for a specific user
    /// </summary>
    Task<AppResult<List<LootDrop>>> GetUserLootDropsAsync(Guid userId);
}
