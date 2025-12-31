namespace ChoreWars.Core.Entities;

public enum DifficultyLevel
{
    Easy,
    Medium,
    Hard
}

public enum QuestType
{
    OneTime,
    Daily,
    Weekly
}

public enum CompletionStatus
{
    Claimed,
    PendingVerification,
    Approved,
    Rejected
}

public enum PurchaseStatus
{
    Pending,
    Approved,
    Rejected
}

public enum ActivityType
{
    QuestCompleted,
    LevelUp,
    LootFound,
    RewardPurchased,
    StatMilestone
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare
}
