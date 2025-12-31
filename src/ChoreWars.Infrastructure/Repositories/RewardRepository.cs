using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreWars.Infrastructure.Repositories;

public class RewardRepository : Repository<Reward>, IRewardRepository
{
    public RewardRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Reward>> GetAvailableRewardsByPartyIdAsync(Guid partyId)
    {
        return await _dbSet
            .Where(r => r.PartyId == partyId && r.IsAvailable)
            .ToListAsync();
    }

    public async Task<IEnumerable<RewardPurchase>> GetPendingPurchasesByPartyIdAsync(Guid partyId)
    {
        return await _context.RewardPurchases
            .Include(rp => rp.Reward)
            .Include(rp => rp.User)
            .Where(rp => rp.Reward!.PartyId == partyId && rp.Status == PurchaseStatus.Pending)
            .ToListAsync();
    }
}
