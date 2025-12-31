using ChoreWars.Core.Entities;
using ChoreWars.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ChoreWars.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public new DbSet<User> Users => Set<User>();
    public DbSet<Party> Parties => Set<Party>();
    public DbSet<Quest> Quests => Set<Quest>();
    public DbSet<QuestCompletion> QuestCompletions => Set<QuestCompletion>();
    public DbSet<LootDrop> LootDrops => Set<LootDrop>();
    public DbSet<Reward> Rewards => Set<Reward>();
    public DbSet<RewardPurchase> RewardPurchases => Set<RewardPurchase>();
    public DbSet<ActivityFeedItem> ActivityFeedItems => Set<ActivityFeedItem>();
    public DbSet<BossBattle> BossBattles => Set<BossBattle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.HasIndex(e => e.Username).IsUnique();

            entity.HasOne(e => e.Party)
                .WithMany(p => p.Members)
                .HasForeignKey(e => e.PartyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Party configuration
        modelBuilder.Entity<Party>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.InviteCode).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.InviteCode).IsUnique();
        });

        // Quest configuration
        modelBuilder.Entity<Quest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();

            entity.HasOne(e => e.Party)
                .WithMany(p => p.Quests)
                .HasForeignKey(e => e.PartyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // QuestCompletion configuration
        modelBuilder.Entity<QuestCompletion>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Quest)
                .WithMany(q => q.Completions)
                .HasForeignKey(e => e.QuestId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.QuestCompletions)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.LootDrop)
                .WithOne(l => l.QuestCompletion)
                .HasForeignKey<LootDrop>(l => l.QuestCompletionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // LootDrop configuration
        modelBuilder.Entity<LootDrop>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.LootDrops)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Reward configuration
        modelBuilder.Entity<Reward>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();

            entity.HasOne(e => e.Party)
                .WithMany(p => p.Rewards)
                .HasForeignKey(e => e.PartyId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RewardPurchase configuration
        modelBuilder.Entity<RewardPurchase>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Reward)
                .WithMany(r => r.Purchases)
                .HasForeignKey(e => e.RewardId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.RewardPurchases)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ActivityFeedItem configuration
        modelBuilder.Entity<ActivityFeedItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Message).IsRequired();
            entity.HasIndex(e => new { e.PartyId, e.CreatedAt });

            entity.HasOne(e => e.Party)
                .WithMany()
                .HasForeignKey(e => e.PartyId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BossBattle configuration
        modelBuilder.Entity<BossBattle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).IsRequired();
            entity.Property(e => e.GroupRewardDescription).IsRequired();

            entity.HasOne(e => e.Party)
                .WithMany(p => p.BossBattles)
                .HasForeignKey(e => e.PartyId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
