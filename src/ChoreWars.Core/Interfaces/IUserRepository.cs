using ChoreWars.Core.Entities;

namespace ChoreWars.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetUserWithStatsAsync(Guid userId);
    Task<IEnumerable<User>> GetPartyMembersAsync(Guid partyId);
    Task<IEnumerable<User>> GetWeeklyLeaderboardAsync(Guid partyId);
}
