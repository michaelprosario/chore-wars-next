using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChoreWars.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    public async Task<User?> GetUserWithStatsAsync(Guid userId)
    {
        return await _dbSet
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<IEnumerable<User>> GetPartyMembersAsync(Guid partyId)
    {
        return await _dbSet
            .Where(u => u.PartyId == partyId)
            .ToListAsync();
    }

    public async Task<IEnumerable<User>> GetWeeklyLeaderboardAsync(Guid partyId)
    {
        // For now, return all party members sorted by level and XP
        // In future, this should be based on weekly XP gains
        return await _dbSet
            .Where(u => u.PartyId == partyId)
            .OrderByDescending(u => u.CurrentLevel)
            .ThenByDescending(u => u.CurrentXP)
            .ToListAsync();
    }
}
