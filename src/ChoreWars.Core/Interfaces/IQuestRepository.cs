using ChoreWars.Core.Entities;

namespace ChoreWars.Core.Interfaces;

public interface IQuestRepository : IRepository<Quest>
{
    Task<IEnumerable<Quest>> GetActiveQuestsByPartyIdAsync(Guid partyId);
    Task<IEnumerable<Quest>> GetAvailableQuestsForUserAsync(Guid userId, Guid partyId);
    Task<Quest?> GetQuestWithCompletionsAsync(Guid questId);
}
