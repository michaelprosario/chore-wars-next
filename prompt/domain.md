# Chore Wars - Domain Model

## Overview

This document provides a comprehensive view of the Chore Wars domain model, including all entities, services, and their relationships.

## Domain Architecture

```mermaid
classDiagram
    %% Core Entities
    class Party {
        +Guid Id
        +string Name
        +string InviteCode
        +Guid DungeonMasterId
        +DateTime CreatedAt
        +ICollection~User~ Members
        +ICollection~Quest~ Quests
        +ICollection~Reward~ Rewards
        +ICollection~BossBattle~ BossBattles
    }

    class User {
        +Guid Id
        +string Username
        +string DisplayName
        +int CurrentLevel
        +int CurrentXP
        +int XPToNextLevel
        +int TotalGold
        +string AvatarUrl
        +Guid PartyId
        +int Strength
        +int Intelligence
        +int Constitution
        +DateTime CreatedAt
        +DateTime LastActiveAt
        +Party Party
        +ICollection~QuestCompletion~ QuestCompletions
        +ICollection~RewardPurchase~ RewardPurchases
        +ICollection~LootDrop~ LootDrops
    }

    class Quest {
        +Guid Id
        +string Title
        +string Description
        +int XPReward
        +int GoldReward
        +DifficultyLevel Difficulty
        +QuestType QuestType
        +int StrengthBonus
        +int IntelligenceBonus
        +int ConstitutionBonus
        +Guid PartyId
        +bool IsActive
        +Guid CreatedByDMId
        +DateTime CreatedAt
        +Party Party
        +ICollection~QuestCompletion~ Completions
    }

    class QuestCompletion {
        +Guid Id
        +Guid QuestId
        +Guid UserId
        +DateTime ClaimedAt
        +DateTime? CompletedAt
        +DateTime? VerifiedAt
        +Guid? VerifiedByDMId
        +CompletionStatus Status
        +int XPEarned
        +int GoldEarned
        +int StrengthGained
        +int IntelligenceGained
        +int ConstitutionGained
        +Quest Quest
        +User User
        +LootDrop LootDrop
    }

    class Reward {
        +Guid Id
        +string Title
        +string Description
        +int GoldCost
        +Guid PartyId
        +bool IsAvailable
        +DateTime CreatedAt
        +Party Party
        +ICollection~RewardPurchase~ Purchases
    }

    class RewardPurchase {
        +Guid Id
        +Guid RewardId
        +Guid UserId
        +int GoldSpent
        +DateTime RequestedAt
        +DateTime? FulfilledAt
        +Guid? FulfilledByDMId
        +PurchaseStatus Status
        +string Memo
        +Reward Reward
        +User User
    }

    class LootDrop {
        +Guid Id
        +string Name
        +string Description
        +Rarity Rarity
        +string IconUrl
        +Guid UserId
        +Guid QuestCompletionId
        +DateTime FoundAt
        +User User
        +QuestCompletion QuestCompletion
    }

    class BossBattle {
        +Guid Id
        +string Name
        +string Description
        +int RequiredTotalXP
        +int CurrentXP
        +Guid PartyId
        +string GroupRewardDescription
        +bool IsActive
        +DateTime StartedAt
        +DateTime? CompletedAt
        +Party Party
    }

    class ActivityFeedItem {
        +Guid Id
        +Guid PartyId
        +Guid UserId
        +ActivityType ActivityType
        +string Message
        +DateTime CreatedAt
        +string Metadata
        +Party Party
        +User User
    }

    %% Enums
    class DifficultyLevel {
        <<enumeration>>
        Easy
        Medium
        Hard
    }

    class QuestType {
        <<enumeration>>
        OneTime
        Daily
        Weekly
    }

    class CompletionStatus {
        <<enumeration>>
        Claimed
        PendingVerification
        Approved
        Rejected
    }

    class PurchaseStatus {
        <<enumeration>>
        Pending
        Approved
        Rejected
    }

    class ActivityType {
        <<enumeration>>
        QuestCompleted
        LevelUp
        LootFound
        RewardPurchased
        StatMilestone
    }

    class Rarity {
        <<enumeration>>
        Common
        Uncommon
        Rare
    }

    %% Services
    class IQuestService {
        <<interface>>
        +ClaimQuestAsync(command) AppResult~QuestCompletionDto~
        +CompleteQuestAsync(command) AppResult~QuestCompletionDto~
        +VerifyQuestAsync(command) AppResult~QuestCompletionDto~
        +GetAvailableQuestsAsync(query) AppResult~List~QuestDto~~
        +GetMyActiveQuestsAsync(query) AppResult~List~QuestDto~~
    }

    class IProgressionService {
        <<interface>>
        +AwardXPAsync(command) AppResult~UserProgressDto~
        +AwardGoldAsync(command) AppResult~UserProgressDto~
        +AwardStatsAsync(command) AppResult~UserProgressDto~
        +CheckLevelUpAsync(command) AppResult~LevelUpDto~
        +GetUserProgressAsync(query) AppResult~UserProgressDto~
        +GetUserStatsAsync(query) AppResult~UserStatsDto~
    }

    %% Relationships
    Party "1" --o "many" User : contains
    Party "1" --o "many" Quest : has
    Party "1" --o "many" Reward : offers
    Party "1" --o "many" BossBattle : hosts
    Party "1" --o "many" ActivityFeedItem : tracks

    User "1" --o "many" QuestCompletion : completes
    User "1" --o "many" RewardPurchase : purchases
    User "1" --o "many" LootDrop : collects
    User "1" --o "many" ActivityFeedItem : generates

    Quest "1" --o "many" QuestCompletion : tracks
    Quest --> DifficultyLevel : has
    Quest --> QuestType : is

    QuestCompletion "1" --o "0..1" LootDrop : may_award
    QuestCompletion --> CompletionStatus : has

    Reward "1" --o "many" RewardPurchase : tracks
    RewardPurchase --> PurchaseStatus : has

    LootDrop --> Rarity : has

    ActivityFeedItem --> ActivityType : categorized_by

    %% Service Dependencies
    IQuestService ..> Quest : manages
    IQuestService ..> QuestCompletion : creates
    IQuestService ..> LootDrop : generates

    IProgressionService ..> User : updates
    IProgressionService ..> ActivityFeedItem : creates
```

## Entity Descriptions

### Core Entities

**Party**
- Represents a household/family group
- Contains the Dungeon Master (DM) who manages quests and approvals
- Unique invite code for joining

**User**
- Individual player in the party
- Tracks RPG stats: Level, XP, Gold, Strength, Intelligence, Constitution
- Maintains avatar and display preferences

**Quest**
- Represents a chore/task to be completed
- Awards XP, Gold, and stat bonuses
- Types: One-Time, Daily, Weekly
- Difficulty levels: Easy, Medium, Hard

**QuestCompletion**
- Tracks the completion lifecycle of a quest
- States: Claimed → Pending Verification → Approved/Rejected
- Records rewards earned (XP, Gold, Stats)
- May trigger a LootDrop (20% chance)

**Reward**
- Real-world rewards purchasable with gold
- Created by the DM (e.g., "Pick next movie", "Sleep in on Saturday")

**RewardPurchase**
- Transaction record for reward redemptions
- Requires DM approval before fulfillment

**LootDrop**
- Virtual collectible items with flavor text
- Randomly awarded on quest completion (20% chance)
- Three rarity levels: Common, Uncommon, Rare

**BossBattle**
- Collaborative party-wide goal
- Accumulates XP from all member quest completions
- Awards group reward when completed

**ActivityFeedItem**
- Event log for party activities
- Displays achievements, level-ups, and milestones

## Services

### IQuestService
Manages the quest lifecycle:
- **ClaimQuestAsync**: Player claims a quest to work on it
- **CompleteQuestAsync**: Player marks quest as done (triggers XP/Gold/Stats/Loot)
- **VerifyQuestAsync**: DM approves or rejects completion
- **GetAvailableQuestsAsync**: Lists unclaimed quests for the party
- **GetMyActiveQuestsAsync**: Lists player's claimed quests

### IProgressionService
Manages player progression and stats:
- **AwardXPAsync**: Grants experience points (may trigger level-up)
- **AwardGoldAsync**: Grants gold currency
- **AwardStatsAsync**: Increases Strength, Intelligence, or Constitution
- **CheckLevelUpAsync**: Determines if user leveled up and calculates new XP threshold
- **GetUserProgressAsync**: Retrieves current progress (level, XP, gold, stats)
- **GetUserStatsAsync**: Retrieves current stat attributes

## Key Business Rules

1. **Quest Lifecycle**: Claimed → Completed → Verified (by DM)
2. **Loot System**: 20% chance on verified quest completion
3. **Stat Attribution**:
   - Strength: Physical chores (groceries, yard work)
   - Intelligence: Mental tasks (bills, planning)
   - Constitution: Routine chores (cooking, laundry)
4. **Level Progression**: Exponential XP scaling
5. **Reward Economy**: Gold-based marketplace with DM approval required
6. **Boss Battles**: Collaborative XP pooling for group achievements

## Enumerations

- **DifficultyLevel**: Easy, Medium, Hard
- **QuestType**: OneTime, Daily, Weekly
- **CompletionStatus**: Claimed, PendingVerification, Approved, Rejected
- **PurchaseStatus**: Pending, Approved, Rejected
- **ActivityType**: QuestCompleted, LevelUp, LootFound, RewardPurchased, StatMilestone
- **Rarity**: Common, Uncommon, Rare
