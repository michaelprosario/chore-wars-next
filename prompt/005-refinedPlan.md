# Chore Wars - Refined Implementation Plan

## Project Overview

**Chore Wars** is a gamified household chore management system that transforms mundane tasks into an engaging RPG-like experience. Players earn XP, level up, collect gold, and compete with family members while completing real-world chores.

### Technology Stack
- **Backend Framework**: ASP.NET MVC
- **Database**: SQLite with Entity Framework Core
- **Authentication**: ASP.NET Core Identity (username/password)
- **Frontend**: Bootstrap 5, JavaScript (ES6+), CSS3
- **Architecture**: Clean Architecture (Ardalis/Smith style)

### Design Principles
- **Mobile-First**: Responsive design optimized for phones and tablets
- **Purple Theme**: Engaging color scheme with purple as primary color
- **Single-Purpose Screens**: Each view focuses on one clear task
- **Instant Feedback**: Visual animations and immediate responses

---

## Core Features Checklist

This plan addresses all required RPG game mechanics for a complete Chore Wars implementation:

### 1. Core Game Mechanics ✅
- ✅ **XP System**: Chores award experience points based on difficulty (15-500 XP range)
- ✅ **Leveling Up**: Exponential XP scaling with "Level Up!" animations and dopamine-inducing feedback
- ✅ **Gold/Currency**: Dual reward system (XP + Gold) on every quest completion
- ✅ **Stat Attributes**: Three RPG stats that grow based on chore type:
  - 💪 **Strength** (physical chores: groceries, yard work, cleaning)
  - 🧠 **Intelligence** (mental chores: bills, planning, organizing)
  - ❤️ **Constitution** (routine chores: cooking, laundry, daily tasks)
- ✅ **Loot Drops**: 20% random chance for silly virtual items ("Crusty Sponge", "Lost TV Remote")

### 2. Quest Board (Task Management) ✅
- ✅ **One-Off Quests**: Single-completion tasks that disappear when done
- ✅ **Daily Quests**: Auto-reset every 24 hours (e.g., "Make your bed")
- ✅ **Weekly Quests**: Auto-reset weekly (e.g., "Mow lawn")
- ✅ **Difficulty Ratings**: Color-coded visual indicators (🟢 Easy | 🟡 Medium | 🔴 Hard)
- ✅ **Claiming Mechanism**: "CLAIM QUEST" prevents duplicate work by multiple players
- ✅ **Quest Types**: Enum-based system (OneTime, Daily, Weekly)

### 3. Party & Social Features ✅
- ✅ **The Party System**: Household groups with invite codes
- ✅ **Leaderboard**: Weekly rankings showing top XP earners (resets Monday)
- ✅ **Activity Feed**: Real-time scrolling log with flavor text:
  - *"Player 1 defeated THE DIRTY LAUNDRY!"*
  - *"Player 2 completed 'Mow the Lawn' (+2 Strength)"*
- ✅ **Boss Battles (Co-op)**: Household goals requiring combined XP (e.g., "Spring Cleaning Dragon" = 5000 total XP)

### 4. Reward Store (Real-World Integration) ✅
- ✅ **DM-Created Rewards**: Custom real-world rewards with gold costs ("Pick the movie" = 50g)
- ✅ **Gold Deduction System**: DM deducts virtual currency when reward is fulfilled
- ✅ **Memo Logging**: DM can add notes when approving/rejecting purchases
- ✅ **Purchase Workflow**: Request → DM Approval → Gold Deduction → Fulfillment

### 5. Administration & Anti-Cheat ✅
- ✅ **Dungeon Master (DM) Role**: Admin rights with full control:
  - Create/edit/delete quests
  - Set XP, Gold, and Stat values
  - Manage reward shop
  - Adjust player stats if needed (with memo)
- ✅ **Verification System**: Two-step anti-cheat workflow:
  1. Player clicks "Complete Quest"
  2. DM receives notification
  3. DM approves (✓) or rejects (✗)
  4. XP/Gold/Stats awarded ONLY upon DM approval
- ✅ **Role-Based Authorization**: `[Authorize(Roles = "DungeonMaster")]` enforcement

### Additional Features
- ✅ **Clean Architecture**: Core/Infrastructure/Web separation for maintainability
- ✅ **CQRS Pattern**: Commands and Queries for clear business logic
- ✅ **Unit Testing**: Comprehensive test coverage for Core services
- ✅ **Mobile-First UI**: Bootstrap 5 responsive design
- ✅ **Purple Theme**: Engaging color scheme as specified
- ✅ **Stat Milestones**: Achievements when reaching stat thresholds (10, 25, 50, 100)
- ✅ **Avatar System**: Visual identity for each player

---

## Architecture Design

### Layer Structure

```
ChoreWars.sln
│
├── ChoreWars.Core/              # Domain & Application Logic (innermost)
│   ├── Entities/                # Domain models
│   ├── Interfaces/              # Abstractions (repositories, services)
│   ├── Services/                # Business logic services
│   ├── Commands/                # CQRS commands
│   ├── Queries/                 # CQRS queries
│   └── Results/                 # Result objects for service responses
│
├── ChoreWars.Infrastructure/    # Data Access & External Concerns
│   ├── Data/                    # EF Core DbContext and configurations
│   ├── Repositories/            # Repository implementations
│   ├── Services/                # Infrastructure service implementations
│   └── Migrations/              # Database migrations
│
└── ChoreWars.Web/               # ASP.NET MVC Web Application
    ├── Controllers/             # MVC Controllers
    ├── Views/                   # Razor views
    ├── wwwroot/                 # Static files (CSS, JS, images)
    └── ViewModels/              # View-specific models
```

### Dependency Rules
1. **Core** has zero dependencies on Infrastructure or Web
2. **Infrastructure** depends on Core (implements interfaces)
3. **Web** depends on Core and Infrastructure (orchestration)
4. All dependencies point **inward** toward Core

---

## Domain Model (Core Entities)

### 1. User (Player)
```csharp
// Core/Entities/User.cs
- Id: Guid
- Username: string
- DisplayName: string
- CurrentLevel: int
- CurrentXP: int
- XPToNextLevel: int
- TotalGold: int
- AvatarUrl: string (optional)
- PartyId: Guid
// Stat Attributes (RPG Stats)
- Strength: int (earned from physical chores: groceries, cleaning, yard work)
- Intelligence: int (earned from mental chores: bills, planning, organizing)
- Constitution: int (earned from routine/maintenance chores: cooking, laundry)
- CreatedAt: DateTime
- LastActiveAt: DateTime
```

### 2. Quest (Chore Task)
```csharp
// Core/Entities/Quest.cs
- Id: Guid
- Title: string
- Description: string
- XPReward: int
- GoldReward: int
- Difficulty: DifficultyLevel (enum: Easy, Medium, Hard)
- QuestType: QuestType (enum: OneTime, Daily, Weekly)
// Stat Bonuses (Optional - can be null/0 if quest doesn't grant stats)
- StrengthBonus: int (e.g., "Carry groceries" = +2 Strength)
- IntelligenceBonus: int (e.g., "Pay bills" = +1 Intelligence)
- ConstitutionBonus: int (e.g., "Cook dinner" = +1 Constitution)
- PartyId: Guid
- IsActive: bool
- CreatedByDMId: Guid
- CreatedAt: DateTime
```

### 3. QuestCompletion
```csharp
// Core/Entities/QuestCompletion.cs
- Id: Guid
- QuestId: Guid
- UserId: Guid
- ClaimedAt: DateTime
- CompletedAt: DateTime?
- VerifiedAt: DateTime?
- VerifiedByDMId: Guid?
- Status: CompletionStatus (enum: Claimed, PendingVerification, Approved, Rejected)
- XPEarned: int
- GoldEarned: int
// Stat Gains (snapshot of what was earned from this quest)
- StrengthGained: int
- IntelligenceGained: int
- ConstitutionGained: int
```

### 4. Party (Household Group)
```csharp
// Core/Entities/Party.cs
- Id: Guid
- Name: string
- InviteCode: string (unique)
- DungeonMasterId: Guid
- CreatedAt: DateTime
```

### 5. LootDrop
```csharp
// Core/Entities/LootDrop.cs
- Id: Guid
- Name: string
- Description: string (flavor text)
- Rarity: Rarity (enum: Common, Uncommon, Rare)
- IconUrl: string
- UserId: Guid
- QuestCompletionId: Guid
- FoundAt: DateTime
```

### 6. Reward
```csharp
// Core/Entities/Reward.cs
- Id: Guid
- Title: string
- Description: string
- GoldCost: int
- PartyId: Guid
- IsAvailable: bool
- CreatedAt: DateTime
```

### 7. RewardPurchase
```csharp
// Core/Entities/RewardPurchase.cs
- Id: Guid
- RewardId: Guid
- UserId: Guid
- GoldSpent: int
- RequestedAt: DateTime
- FulfilledAt: DateTime?
- FulfilledByDMId: Guid?
- Status: PurchaseStatus (enum: Pending, Approved, Rejected)
- Memo: string?
```

### 8. ActivityFeedItem
```csharp
// Core/Entities/ActivityFeedItem.cs
- Id: Guid
- PartyId: Guid
- UserId: Guid
- ActivityType: ActivityType (enum: QuestCompleted, LevelUp, LootFound, RewardPurchased, StatMilestone)
- Message: string (flavor text)
- CreatedAt: DateTime
- Metadata: string (JSON - stores additional context like which stat increased)
```

**Example Activity Messages:**
- "Alex vanquished THE OVERFLOWING TRASH! (+50 XP, +25 Gold, +2 Strength)"
- "Sam's Intelligence reached 50! Master of Household Management!"
- "Jordan completed 'Cook Dinner' (+1 Constitution)"

### 9. BossBattle (Optional Feature)
```csharp
// Core/Entities/BossBattle.cs
- Id: Guid
- Name: string
- Description: string
- RequiredTotalXP: int
- CurrentXP: int
- PartyId: Guid
- GroupRewardDescription: string
- IsActive: bool
- StartedAt: DateTime
- CompletedAt: DateTime?
```

---

## RPG Stat Attributes System

### Overview
The stat system adds depth to progression by rewarding different **types** of chores with different **character attributes**. This mirrors traditional RPG mechanics where completing certain activities improves specific skills.

### The Three Core Stats

| Stat | Description | Example Chores | Visual Theme |
|------|-------------|----------------|--------------|
| **Strength** | Physical prowess and manual labor | Carrying groceries, moving furniture, yard work, cleaning garage, taking out trash | 💪 Red/Orange theme |
| **Intelligence** | Mental tasks and planning | Paying bills, meal planning, organizing files, scheduling appointments, budgeting | 🧠 Blue/Cyan theme |
| **Constitution** | Routine maintenance and endurance | Cooking meals, doing laundry, daily cleaning, walking pets, watering plants | ❤️ Green theme |

### How Stats Work

**1. Quest Creation (DM)**
- When creating a quest, DM assigns XP, Gold, AND optional stat bonuses
- Example: "Carry Groceries" → 50 XP, 25 Gold, +2 Strength
- Example: "Pay Monthly Bills" → 75 XP, 30 Gold, +3 Intelligence
- Example: "Cook Dinner" → 40 XP, 20 Gold, +1 Constitution

**2. Quest Completion (Player)**
- When a quest is verified, the player receives:
  - XP (contributes to leveling)
  - Gold (used for reward shop)
  - Stat bonuses (permanent character growth)

**3. Stat Tracking**
- Stats accumulate over time (no cap)
- Players can see their stat totals on their profile
- Activity feed shows stat gains: *"Alex gained +2 Strength!"*

**4. Stat Milestones (Optional Enhancement)**
- Celebrate when players reach milestones (10, 25, 50, 100, 250, etc.)
- Display badges or titles:
  - Strength 50 = "Household Warrior"
  - Intelligence 50 = "Master Planner"
  - Constitution 100 = "Chore Marathoner"

### Implementation Notes

**Flexibility:**
- Not all quests need to grant stats (some can be XP/Gold only)
- DM has full control over which stats each quest awards
- Stats are purely cosmetic/motivational (no in-game mechanical effects)

**Balance:**
- Recommend similar stat distribution across quest types
- Avoid creating "stat farming" scenarios
- Keep stat bonuses proportional to XP rewards (harder chores = more stats)

**Display:**
- Show stats prominently on player profile
- Include stats in activity feed messages
- Display stat gains in quest completion animations

---

## Core Services & CQRS Pattern

### Service Examples

#### 1. QuestService (Core/Services/QuestService.cs)
**Commands:**
- `ClaimQuestCommand` → `AppResult<QuestCompletionDto>`
- `CompleteQuestCommand` → `AppResult<QuestCompletionDto>`
- `VerifyQuestCommand` → `AppResult<QuestCompletionDto>`

**Queries:**
- `GetAvailableQuestsQuery` → `AppResult<List<QuestDto>>`
- `GetMyActiveQuestsQuery` → `AppResult<List<QuestDto>>`

#### 2. ProgressionService (Core/Services/ProgressionService.cs)
**Commands:**
- `AwardXPCommand` → `AppResult<UserProgressDto>`
- `AwardGoldCommand` → `AppResult<UserProgressDto>`
- `AwardStatsCommand` → `AppResult<UserProgressDto>` (handles Str/Int/Con)
- `CheckLevelUpCommand` → `AppResult<LevelUpDto>` (triggers on XP threshold)

**Queries:**
- `GetUserProgressQuery` → `AppResult<UserProgressDto>`
- `GetUserStatsQuery` → `AppResult<UserStatsDto>`

**Business Logic:**
- Calculate XP thresholds for leveling (e.g., Level 1→2 = 100 XP, Level 2→3 = 250 XP, exponential)
- Award stats based on quest completion
- Track stat milestones (every 10, 25, 50, 100 points)

#### 3. RewardService (Core/Services/RewardService.cs)
**Commands:**
- `PurchaseRewardCommand` → `AppResult<RewardPurchaseDto>`
- `ApproveRewardPurchaseCommand` → `AppResult<RewardPurchaseDto>`

**Queries:**
- `GetAvailableRewardsQuery` → `AppResult<List<RewardDto>>`

### AppResult Pattern
```csharp
// Core/Results/AppResult.cs
public class AppResult<T>
{
    public bool IsSuccess { get; set; }
    public T Data { get; set; }
    public List<string> Errors { get; set; }
    public List<ValidationError> ValidationErrors { get; set; }
    public string Message { get; set; }
}
```

---

## Implementation Phases

### Phase 1: Foundation Setup (Week 1)

**1.1 Project Structure**
- Create solution with three projects (Core, Infrastructure, Web)
- Set up project references
- Configure dependency injection in Web project

**1.2 Database & Authentication**
- Install packages: EF Core, SQLite provider, Identity
- Create `ApplicationDbContext` in Infrastructure
- Extend `IdentityUser` with game properties
- Set up ASP.NET Core Identity authentication
- Create initial migration

**1.3 Core Domain Entities**
- Implement all entity classes in Core/Entities
- Define enums (DifficultyLevel, QuestType, etc.)
- Create entity configurations for EF Core

**Deliverables:**
- ✅ Working database with tables
- ✅ User registration and login functional
- ✅ Project structure follows Clean Architecture

---

### Phase 2: Quest System (Week 2)

**2.1 Quest Management (DM Features)**
- Create `IQuestRepository` interface in Core
- Implement `QuestRepository` in Infrastructure
- Build `QuestService` with Command/Query methods
- Create DM controller and views:
  - Quest creation form (Title, Description, XP, Gold, Difficulty, Type, Stat Bonuses)
  - Quest listing/editing
  - Quest deletion

**2.2 Quest Board (Player Features)**
- Create player quest board view
- Implement quest claiming logic
- Build quest completion workflow
- Add quest verification for DM

**2.3 Testing**
- Unit tests for `QuestService`
- Integration tests for quest workflows

**Deliverables:**
- ✅ DM can create/edit/delete quests
- ✅ Players can view, claim, and complete quests
- ✅ DM can verify completions

---

### Phase 3: Progression System (Week 3)

**3.1 XP & Leveling**
- Implement `ProgressionService` in Core
- Create leveling algorithm (e.g., exponential scaling)
- Build XP progress bar component
- Add "Level Up!" animation

**3.2 Gold Economy**
- Implement gold earning on quest completion
- Track user gold balance
- Create gold transaction history

**3.3 Loot Drop System**
- Implement 20% random loot drop logic
- Create loot table with funny items
- Display loot drops in activity feed
- Build user inventory view

**3.4 Stat Attributes System**
- Add Strength, Intelligence, Constitution fields to User entity
- Update Quest creation form to include stat bonuses
- Modify `ProgressionService` to award stats on quest verification
- Create stat display components for player profile
- Add stat gains to activity feed messages
- Implement stat milestone tracking (optional)
- Display stat breakdown with themed colors (STR=Red, INT=Blue, CON=Green)

**Deliverables:**
- ✅ Players earn XP and level up
- ✅ Gold is awarded and tracked
- ✅ Random loot drops occur with flavor text
- ✅ Stat attributes are tracked and displayed
- ✅ Quests can award stat bonuses

---

### Phase 4: Party & Social Features (Week 4)

**4.1 Party System**
- Create party creation and invite system
- Build party dashboard view
- Display all party members with levels/avatars

**4.2 Leaderboard**
- Implement weekly leaderboard logic
- Create leaderboard view (resets Monday)
- Display rankings prominently

**4.3 Activity Feed**
- Implement `ActivityFeedService`
- Generate flavor text for activities
- Display real-time feed on dashboard

**Deliverables:**
- ✅ Parties can be created and joined
- ✅ Weekly leaderboard functional
- ✅ Activity feed shows recent events

---

### Phase 5: Reward Shop (Week 5)

**5.1 Reward Creation (DM)**
- Build reward management interface
- Create/edit/delete rewards
- Set gold costs for each reward

**5.2 Reward Purchase (Player)**
- Create reward shop view
- Implement purchase request workflow
- Build purchase confirmation

**5.3 Reward Fulfillment (DM)**
- Create DM notification system for purchases
- Build approval/rejection interface
- Deduct gold on approval
- Add memo capability

**Deliverables:**
- ✅ DM can create custom rewards
- ✅ Players can browse and purchase rewards
- ✅ DM can approve/reject with memos

---

### Phase 6: Boss Battles (Optional) (Week 6)

**6.1 Boss Battle Creation**
- Create boss battle entity
- Build DM interface for boss creation
- Set XP requirements and group rewards

**6.2 Progress Tracking**
- Track combined party XP toward boss
- Display progress bar on dashboard
- Trigger completion when threshold met

**6.3 Celebration**
- Create victory animation
- Distribute group rewards
- Add to activity feed

**Deliverables:**
- ✅ Boss battles can be created
- ✅ Party XP contributes to boss defeat
- ✅ Group rewards are distributed

---

### Phase 7: Polish & UX (Week 7)

**7.1 Mobile Optimization**
- Refine responsive layouts
- Ensure large, tappable buttons
- Test on multiple device sizes

**7.2 Animations & Feedback**
- Add completion animations
- Implement sound effects (optional)
- Create celebratory visuals for level-ups

**7.3 Avatar System**
- Create simple avatar selection
- Display avatars throughout app

**7.4 Notifications (Future Enhancement)**
- Plan push notification system
- Email weekly recap (optional)

**Deliverables:**
- ✅ Smooth mobile experience
- ✅ Engaging animations
- ✅ Avatar customization

---

### Phase 8: Testing & Deployment (Week 8)

**8.1 Comprehensive Testing**
- Unit tests for all Core services
- Integration tests for critical workflows
- Manual QA testing

**8.2 Security Review**
- Validate authentication flows
- Check authorization rules (DM vs Player)
- Review for SQL injection, XSS vulnerabilities

**8.3 Deployment Preparation**
- Set up production database
- Configure environment variables
- Create deployment documentation

**Deliverables:**
- ✅ All tests passing
- ✅ Security validated
- ✅ Ready for production deployment

---

## User Interface Design Guidelines

### Color Scheme (Purple Theme)
**Primary Colors:**
- **Deep Purple**: `#6A0DAD` (primary actions, headers)
- **Light Purple**: `#9B4DCA` (secondary elements)
- **Accent Purple**: `#C77DFF` (highlights, progress bars)

**Supporting Colors:**
- **Gold**: `#FFD700` (gold currency, rewards)
- **Success Green**: `#28A745` (completions, approvals)
- **Warning Red**: `#DC3545` (rejections, urgent)
- **Dark Background**: `#2D1B4E` (cards, sections)
- **Light Background**: `#F8F9FA` (page background)

**Stat Colors (RPG Attributes):**
- **Strength (STR)**: `#FF6B6B` (red/orange for physical power)
- **Intelligence (INT)**: `#4ECDC4` (cyan/blue for mental prowess)
- **Constitution (CON)**: `#95E1D3` (green for endurance)

### Typography
- **Headings**: Bold, clear hierarchy (H1: 2.5rem, H2: 2rem, H3: 1.5rem)
- **Body Text**: 16px minimum for readability
- **Button Text**: 18px, bold, high contrast

### Component Design

#### Quest Cards
```
┌─────────────────────────────────┐
│ 🟡 MEDIUM                       │
│ Take Out the Trash              │
│ ─────────────────────────────   │
│ XP: 50  |  Gold: 25             │
│ 💪 STR +2                       │
│                                 │
│ [    CLAIM QUEST    ]           │
└─────────────────────────────────┘

With Multiple Stats:
┌─────────────────────────────────┐
│ 🔴 HARD                         │
│ Deep Clean Kitchen              │
│ ─────────────────────────────   │
│ XP: 150  |  Gold: 75            │
│ 💪 STR +3  |  ❤️ CON +2        │
│                                 │
│ [    CLAIM QUEST    ]           │
└─────────────────────────────────┘
```

#### Progress Bar
```
Level 7 ━━━━━━━━━━━━━━━░░░░░ 750/1000 XP
         ▲ 75% to Level 8
```

#### Activity Feed Item
```
┌─────────────────────────────────┐
│ 👤 Alex  •  2 minutes ago       │
│ ⚔️  Vanquished THE OVERFLOWING  │
│    TRASH!                       │
│ +50 XP, +25 Gold, 💪+2 STR     │
│ 🎁 Found: Crusty Sponge         │
└─────────────────────────────────┘

Stat Milestone Example:
┌─────────────────────────────────┐
│ 👤 Sam  •  1 hour ago           │
│ 🧠 Intelligence reached 50!     │
│ Earned title: "Master Planner"  │
└─────────────────────────────────┘
```

#### Player Stats Display (Profile Card)
```
┌─────────────────────────────────┐
│ 👤 Alex - Level 7               │
│ ━━━━━━━━━━━━░░░░ 750/1000 XP   │
│                                 │
│ ATTRIBUTES                      │
│ 💪 Strength       42            │
│ ██████████░░░░░░░░░░            │
│                                 │
│ 🧠 Intelligence   28            │
│ ██████░░░░░░░░░░░░░░            │
│                                 │
│ ❤️ Constitution   35            │
│ ████████░░░░░░░░░░░░            │
│                                 │
│ 💰 Gold: 450                    │
└─────────────────────────────────┘
```

### Mobile-First Views

**Quest Board (Mobile)**
- Single column layout
- Large tap targets (minimum 44px height)
- Swipe gestures for details
- Fixed bottom navigation

**Party Dashboard (Mobile)**
- Vertical stack of member cards
- Collapsible leaderboard
- Pull-to-refresh activity feed

**Reward Shop (Mobile)**
- Grid layout (2 columns)
- Price prominently displayed
- Quick purchase modal

---

## Testing Strategy

### Unit Tests (Core Project)
**Framework**: xUnit

**Test Coverage:**
1. **QuestService Tests**
   - Test quest claiming logic
   - Test quest completion validation
   - Test quest verification workflow

2. **ProgressionService Tests**
   - Test XP calculation
   - Test leveling algorithm
   - Test gold transactions

3. **RewardService Tests**
   - Test reward purchase validation
   - Test gold deduction logic
   - Test purchase approval workflow

**Example Test:**
```csharp
[Fact]
public async Task ClaimQuest_WhenQuestAvailable_ReturnsSuccess()
{
    // Arrange
    var command = new ClaimQuestCommand { QuestId = questId, UserId = userId };

    // Act
    var result = await _questService.ClaimQuestAsync(command);

    // Assert
    Assert.True(result.IsSuccess);
    Assert.NotNull(result.Data);
}
```

### Integration Tests
**Framework**: ASP.NET Core Testing Tools

**Test Scenarios:**
1. End-to-end quest workflow (claim → complete → verify)
2. User registration and authentication
3. Party creation and joining
4. Reward purchase and fulfillment

### Manual QA Checklist
- [ ] Mobile responsiveness (iOS Safari, Android Chrome)
- [ ] Cross-browser compatibility (Chrome, Firefox, Safari, Edge)
- [ ] Accessibility (keyboard navigation, screen readers)
- [ ] Performance (page load times < 2 seconds)

---

## Security Considerations

### Authentication & Authorization
1. **ASP.NET Core Identity**
   - Secure password hashing (default PBKDF2)
   - Username/password authentication
   - Session management

2. **Role-Based Access**
   - **DM Role**: Quest/reward management, verification
   - **Player Role**: Quest claiming, completion, purchases
   - Use `[Authorize(Roles = "DungeonMaster")]` attribute

### Input Validation
1. **Command Validation**
   - Validate all command objects before processing
   - Use Data Annotations for model validation
   - Return validation errors in AppResult

2. **SQL Injection Prevention**
   - Use EF Core parameterized queries (default)
   - Never concatenate user input into SQL

3. **XSS Prevention**
   - Razor automatically encodes output
   - Sanitize user-generated content (quest descriptions, etc.)
   - Use `Html.Raw()` only for trusted content

### Business Logic Security
1. **Quest Claiming**
   - Prevent users from claiming already-claimed quests
   - Verify user belongs to quest's party

2. **Gold Transactions**
   - Validate sufficient balance before purchases
   - Use database transactions for atomic operations

3. **DM Verification**
   - Only party DM can verify completions
   - Only party DM can approve reward purchases

---

## Database Schema

### SQLite Database File
**Location**: `App_Data/chorewars.db`

### Key Indexes
```sql
CREATE INDEX IX_QuestCompletions_UserId ON QuestCompletions(UserId);
CREATE INDEX IX_QuestCompletions_QuestId ON QuestCompletions(QuestId);
CREATE INDEX IX_ActivityFeedItems_PartyId_CreatedAt ON ActivityFeedItems(PartyId, CreatedAt DESC);
CREATE INDEX IX_Users_PartyId ON Users(PartyId);
```

### Entity Framework Configuration Example
```csharp
// Infrastructure/Data/Configuration/QuestConfiguration.cs
public class QuestConfiguration : IEntityTypeConfiguration<Quest>
{
    public void Configure(EntityTypeBuilder<Quest> builder)
    {
        builder.HasKey(q => q.Id);
        builder.Property(q => q.Title).IsRequired().HasMaxLength(100);
        builder.Property(q => q.XPReward).IsRequired();
        builder.Property(q => q.GoldReward).IsRequired();

        builder.HasOne(q => q.Party)
               .WithMany(p => p.Quests)
               .HasForeignKey(q => q.PartyId);
    }
}
```

---

## Dependency Injection Setup

### Program.cs Configuration
```csharp
// ChoreWars.Web/Program.cs

// Core Services
builder.Services.AddScoped<IQuestService, QuestService>();
builder.Services.AddScoped<IProgressionService, ProgressionService>();
builder.Services.AddScoped<IRewardService, RewardService>();
builder.Services.AddScoped<IActivityFeedService, ActivityFeedService>();

// Infrastructure Repositories
builder.Services.AddScoped<IQuestRepository, QuestRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRewardRepository, RewardRepository>();

// Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddDefaultIdentity<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

---

## Critical Success Factors

### Technical Excellence
1. ✅ **Clean Architecture Compliance**: Strict adherence to dependency rules
2. ✅ **Comprehensive Testing**: >80% code coverage for Core services
3. ✅ **Mobile Performance**: Fast load times, smooth animations

### User Experience
1. ✅ **Instant Gratification**: Immediate feedback on all actions
2. ✅ **Visual Clarity**: Clear progress indicators and status
3. ✅ **Frictionless Flow**: 2-tap quest completion (Claim → Complete)

### Game Balance
1. ✅ **Fair XP Scaling**: Meaningful progression without grind
2. ✅ **Engaging Rewards**: Real-world incentives that matter
3. ✅ **Fun Competition**: Encouraging without toxic pressure

---

## Future Enhancements (Post-MVP)

### Phase 9+
1. **Push Notifications** (when quests available, verification needed)
2. **Weekly Recap Emails** (XP earned, achievements unlocked)
3. **Mobile Apps** (iOS/Android with Xamarin or MAUI)
4. **Advanced Statistics** (charts, trends, completion rates)
5. **Seasonal Events** (holiday-themed quests and rewards)
6. **Achievement Badges** (milestone tracking)
7. **Quest Templates** (pre-made quest library)
8. **Photo Verification** (upload proof of completion)

---

## Conclusion

This refined plan provides a comprehensive roadmap for building **Chore Wars** as a production-ready ASP.NET MVC application following Clean Architecture principles. The 8-phase approach balances technical rigor with user-focused features, ensuring both code quality and engaging gameplay.

### Next Steps
1. ✅ Review and approve this implementation plan
2. Set up development environment
3. Begin Phase 1: Foundation Setup
4. Establish weekly sprint reviews
5. Iterate based on user feedback

**Estimated Timeline**: 8-10 weeks to MVP (Phases 1-5)
**Recommended Team**: 1-2 developers, 1 UX designer (part-time)

---

*This plan synthesizes Clean Architecture best practices with gamification principles to create a maintainable, testable, and delightful application.*
