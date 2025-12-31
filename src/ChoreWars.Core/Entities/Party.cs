namespace ChoreWars.Core.Entities;

public class Party
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string InviteCode { get; set; } // Unique invite code
    public Guid DungeonMasterId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<User> Members { get; set; } = new List<User>();
    public ICollection<Quest> Quests { get; set; } = new List<Quest>();
    public ICollection<Reward> Rewards { get; set; } = new List<Reward>();
    public ICollection<BossBattle> BossBattles { get; set; } = new List<BossBattle>();
}
