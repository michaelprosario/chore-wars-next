using ChoreWars.Core.Entities;

namespace ChoreWars.Core.Interfaces;

public interface IRewardRepository : IRepository<Reward>
{
    Task<IEnumerable<Reward>> GetAvailableRewardsByPartyIdAsync(Guid partyId);
    Task<IEnumerable<RewardPurchase>> GetPendingPurchasesByPartyIdAsync(Guid partyId);
}
