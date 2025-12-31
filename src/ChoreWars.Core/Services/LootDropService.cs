using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Core.Results;

namespace ChoreWars.Core.Services;

public class LootDropService : ILootDropService
{
    private readonly IRepository<LootDrop> _lootDropRepository;
    private readonly Random _random = new();

    private static readonly List<(string Name, string Description, Rarity Rarity)> LootTable = new()
    {
        // Common items (60%)
        ("Crusty Sponge", "It's seen better days, but it's yours now!", Rarity.Common),
        ("Lost TV Remote", "Found between the couch cushions. Still sticky.", Rarity.Common),
        ("Mismatched Sock", "Its partner is lost to the ages.", Rarity.Common),
        ("Dust Bunny", "A fluffy companion from under the bed.", Rarity.Common),
        ("Empty Pen", "Ran out of ink at the worst possible moment.", Rarity.Common),
        ("Expired Coupon", "Good until last month. So close!", Rarity.Common),
        ("Mystery Stain", "We don't ask questions about this one.", Rarity.Common),
        ("Bent Paperclip", "Once held important documents together.", Rarity.Common),
        ("Crumb Collection", "Archaeological evidence of last week's snacks.", Rarity.Common),
        ("Forgotten Receipt", "From a store that no longer exists.", Rarity.Common),

        // Uncommon items (30%)
        ("Golden Spatula", "Flips pancakes with legendary precision!", Rarity.Uncommon),
        ("Enchanted Feather Duster", "+5 to cleaning speed!", Rarity.Uncommon),
        ("Magical Laundry Basket", "Never overflows, somehow.", Rarity.Uncommon),
        ("Singing Vacuum Cleaner", "Hums a cheerful tune while working.", Rarity.Uncommon),
        ("Self-Organizing Drawer", "Items arrange themselves. Probably.", Rarity.Uncommon),
        ("Lucky Dish Towel", "Dishes almost wash themselves!", Rarity.Uncommon),
        ("Perpetual Calendar", "Always shows the right date. Magic!", Rarity.Uncommon),

        // Rare items (10%)
        ("Sword of Dish Slaying", "Legendary weapon against dirty plates!", Rarity.Rare),
        ("Crown of the Chore Champion", "Worn by household heroes!", Rarity.Rare),
        ("Staff of Infinite Motivation", "+100 willpower to do chores!", Rarity.Rare),
        ("Cape of Stain Resistance", "Spills fear this legendary garment!", Rarity.Rare),
        ("Amulet of Time Management", "Grants the power of productivity!", Rarity.Rare)
    };

    public LootDropService(IRepository<LootDrop> lootDropRepository)
    {
        _lootDropRepository = lootDropRepository;
    }

    public async Task<AppResult<LootDrop?>> TryGenerateLootDropAsync(
        Guid userId,
        Guid questCompletionId)
    {
        // 20% chance for loot drop
        if (_random.NextDouble() > 0.20)
        {
            // No loot this time
            return AppResult<LootDrop?>.Success(null, "No loot dropped this time");
        }

        // Determine rarity based on weighted probability
        // Common: 60%, Uncommon: 30%, Rare: 10%
        var rarityRoll = _random.NextDouble();
        Rarity targetRarity;

        if (rarityRoll < 0.60)
            targetRarity = Rarity.Common;
        else if (rarityRoll < 0.90)
            targetRarity = Rarity.Uncommon;
        else
            targetRarity = Rarity.Rare;

        // Filter loot table by rarity and pick random item
        var availableLoot = LootTable.Where(l => l.Rarity == targetRarity).ToList();
        var selectedLoot = availableLoot[_random.Next(availableLoot.Count)];

        var lootDrop = new LootDrop
        {
            Id = Guid.NewGuid(),
            Name = selectedLoot.Name,
            Description = selectedLoot.Description,
            Rarity = selectedLoot.Rarity,
            UserId = userId,
            QuestCompletionId = questCompletionId,
            FoundAt = DateTime.UtcNow
        };

        await _lootDropRepository.AddAsync(lootDrop);
        await _lootDropRepository.SaveChangesAsync();

        return AppResult<LootDrop?>.Success(
            lootDrop,
            $"Legendary loot found: {lootDrop.Name}!");
    }

    public async Task<AppResult<List<LootDrop>>> GetUserLootDropsAsync(Guid userId)
    {
        var lootDrops = await _lootDropRepository.GetAllByConditionAsync(
            l => l.UserId == userId);

        var sortedLootDrops = lootDrops
            .OrderByDescending(l => l.FoundAt)
            .ToList();

        return AppResult<List<LootDrop>>.Success(sortedLootDrops);
    }
}
