using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreWars.Infrastructure.Repositories;

public class QuestRepository : Repository<Quest>, IQuestRepository
{
    public QuestRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Quest>> GetActiveQuestsByPartyIdAsync(Guid partyId)
    {
        return await _dbSet
            .Where(q => q.PartyId == partyId && q.IsActive)
            .ToListAsync();
    }

    public async Task<IEnumerable<Quest>> GetAvailableQuestsForUserAsync(Guid userId, Guid partyId)
    {
        // Get quests that are active and belong to the party
        // and are not currently claimed by this user or pending verification
        var claimedQuestIds = await _context.QuestCompletions
            .Where(qc => qc.Status == CompletionStatus.Claimed || qc.Status == CompletionStatus.PendingVerification)
            .Select(qc => qc.QuestId)
            .ToListAsync();

        return await _dbSet
            .Where(q => q.PartyId == partyId && q.IsActive && !claimedQuestIds.Contains(q.Id))
            .ToListAsync();
    }

    public async Task<Quest?> GetQuestWithCompletionsAsync(Guid questId)
    {
        return await _dbSet
            .Include(q => q.Completions)
            .FirstOrDefaultAsync(q => q.Id == questId);
    }
}
