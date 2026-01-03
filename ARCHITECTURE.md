# ChoreWars Architecture Documentation

## Overview

ChoreWars is a gamified chore management system built using **Clean Architecture** principles with ASP.NET Core 10.0. The application transforms household tasks into an RPG-style gaming experience where users can complete quests (chores), earn experience points, level up, collect loot, and compete on leaderboards.

---

## Architecture Pattern: Clean Architecture

The application follows **Clean Architecture** (also known as Onion Architecture) principles advocated by Steve Smith (Ardalis), ensuring:

- **Dependency Inversion**: All dependencies point inward toward the core business logic
- **Framework Independence**: Core business logic is decoupled from external frameworks
- **Testability**: Core logic can be unit tested without infrastructure dependencies
- **Maintainability**: Clear separation of concerns across layers

### Core Principles

1. **Dependencies point inward** - Outer layers depend on inner layers, never the reverse
2. **Core independence** - The Core project has zero dependencies on Infrastructure or Web layers
3. **Interface-based design** - Core defines interfaces; Infrastructure implements them
4. **CQRS pattern** - Commands and Queries are separate concerns with dedicated objects
5. **Result objects** - Services return `AppResult` objects for consistent error handling

---

## Project Structure

The solution consists of three main projects organized in concentric layers:

```
ChoreWars Solution
├── src/
│   ├── ChoreWars.Core/          # Inner Layer - Business Logic
│   ├── ChoreWars.Infrastructure/ # Middle Layer - Data & External Services
│   └── ChoreWars.Web/            # Outer Layer - UI & API
└── tests/
    └── ChoreWars.Core.Tests/     # Unit Tests
```

---

## Layer Details

### 1. ChoreWars.Core (Domain & Application Layer)

**Purpose**: Contains all business logic, domain entities, and application use cases. This layer has **no external dependencies**.

**Key Components**:

#### Entities (`Entities/`)
Domain models representing the core business concepts:
- `User` - Player character with XP, level, stats (Strength, Intelligence, Constitution), and gold
- `Party` - Household groups with invite codes for member management
- `Quest` - Chores/tasks with XP/gold rewards, difficulty levels, and stat bonuses
- `QuestCompletion` - Records of completed quests with verification status
- `LootDrop` - Random items earned from quest completions (20% drop rate)
- `Reward` - Purchasable items in the reward shop
- `RewardPurchase` - Transaction records for reward purchases
- `ActivityFeedItem` - Activity log entries for party feed
- `BossBattle` - Special collaborative challenges (future feature)
- `Enums` - DifficultyLevel, QuestType, ActivityType, etc.

#### Interfaces (`Interfaces/`)
Abstractions for external dependencies:
- `IQuestRepository` - Quest data access
- `IUserRepository` - User data access
- `IRewardRepository` - Reward data access
- `IRepository<T>` - Generic repository pattern
- `IQuestService` - Quest business logic
- `IProgressionService` - XP, leveling, and stats logic
- `IActivityFeedService` - Activity tracking
- `ILootDropService` - Loot generation logic

#### Services (`Services/`)
Business logic implementations:
- `QuestService` - Quest management (creation, claiming, completion)
- `ProgressionService` - XP calculation, leveling, stat progression
- `ActivityFeedService` - Activity feed generation and filtering
- `LootDropService` - Random loot generation (20% chance, tier-based rewards)

#### Commands & Queries (`Commands/`, `Queries/`)
CQRS pattern implementation:
- **Commands**: `ClaimQuestCommand`, `CompleteQuestCommand`, `CreateQuestCommand`
- **Queries**: `GetAvailableQuestsQuery`, `GetClaimedQuestsQuery`, `GetQuestByIdQuery`

#### Results (`Results/`)
- `AppResult<T>` - Standardized result object with success/failure states, data, and validation messages

---

### 2. ChoreWars.Infrastructure (Data Access & External Services)

**Purpose**: Implements interfaces defined in Core. Handles database access, identity management, and external integrations.

**Key Components**:

#### Data (`Data/`)
- `ApplicationDbContext` - EF Core DbContext extending IdentityDbContext
  - Manages all entity DbSets (Users, Quests, Parties, etc.)
  - Configures entity relationships and constraints
  - Handles database migrations
- `DbInitializer` - Seeds initial data (roles, default admin user)

#### Identity (`Identity/`)
- `ApplicationUser` - ASP.NET Core Identity user linked to domain User entity
- Role-based authorization (Player, DungeonMaster)

#### Repositories (`Repositories/`)
Concrete implementations of repository interfaces:
- `QuestRepository` - Quest CRUD operations with filtering
- `UserRepository` - User management with party relationships
- `RewardRepository` - Reward catalog management
- `Repository<T>` - Generic repository for common entities

#### Migrations (`Migrations/`)
- Entity Framework Core database migrations
- Schema version control

**Database**: PostgreSQL (migrated from SQLite)

---

### 3. ChoreWars.Web (Presentation Layer)

**Purpose**: ASP.NET Core MVC web application. Handles HTTP requests, user interactions, and view rendering.

**Key Components**:

#### Controllers (`Controllers/`)
MVC controllers managing user workflows:
- `HomeController` - Landing page and dashboard
- `QuestController` - Quest board, claiming, completion
- `DMController` - Dungeon Master quest management and import
- `LeaderboardController` - Party rankings and statistics
- `AccountController` - Authentication (register, login, logout)

#### Models (`Models/`)
View models for MVC views (DTOs for presentation layer)

#### Views (`Views/`)
Razor views for rendering HTML:
- Responsive design with Bootstrap
- Dark mode support
- Card-based layouts for quests
- Real-time feedback UI

#### Program.cs
Application startup configuration:
- Dependency injection container setup
- Database context registration (PostgreSQL with Npgsql)
- ASP.NET Core Identity configuration
- Repository and service registration
- Middleware pipeline (Authentication, Authorization, Routing)
- Database initialization and migration on startup

---

## Technology Stack

### Core Technologies
- **.NET 10.0** - Runtime and framework
- **ASP.NET Core MVC** - Web framework
- **Entity Framework Core** - ORM for data access
- **PostgreSQL** - Relational database (via Npgsql)
- **ASP.NET Core Identity** - Authentication and authorization

### Frontend
- **Razor Views** - Server-side rendering
- **Bootstrap 5** - Responsive UI framework
- **Dark Mode** - Custom CSS theming
- **jQuery** - Client-side interactivity

### Development & Deployment
- **Docker** - Containerization
- **Docker Compose** - Multi-container orchestration (app + PostgreSQL)
- **xUnit** - Unit testing framework

---

## Key Features & Domain Logic

### RPG Progression System
- **Leveling**: Exponential XP curve (100 * level² XP required)
- **Character Stats**: Strength, Intelligence, Constitution with milestone achievements (10, 25, 50, 100, 250, 500)
- **Loot System**: 20% chance for random loot drops on quest completion
- **Gold Economy**: Earn gold from quests, spend in reward shop

### Quest Management
- **Quest Types**: Daily, Weekly, One-Time
- **Difficulty Levels**: Easy (50 XP), Medium (100 XP), Hard (200 XP)
- **Stat Bonuses**: Quests can grant permanent stat increases
- **Verification Workflow**: DM must verify quest completions before rewards are granted
- **Bulk Import**: JSON-based quest import for DMs

### Party System
- **Invite Codes**: 6-character codes for easy party joining
- **Activity Feed**: Real-time updates on party member activities
- **Leaderboard**: Party-wide rankings by XP or Gold with top 3 highlighting

### Role-Based Access Control
- **Player**: Claim quests, complete quests, view leaderboard, track progress
- **Dungeon Master (DM)**: All Player permissions plus create/manage quests, verify completions, import quests

---

## Data Flow

### Typical Quest Completion Flow

```
1. User (Player) → QuestController → IQuestService
2. QuestService → IQuestRepository (check quest availability)
3. QuestService → Claim Quest → Save to DB
4. User Completes Chore → Mark as Complete (pending verification)
5. DM Reviews → Verify Completion
6. QuestService → IProgressionService (award XP, gold, stats)
7. ProgressionService → Level Up Logic + Stat Calculation
8. LootDropService → Roll for Loot (20% chance)
9. ActivityFeedService → Log Activity
10. Save all changes → DB commit
11. Redirect to Dashboard with success message
```

---

## Design Patterns

### Repository Pattern
- Abstracts data access logic
- Defined in Core, implemented in Infrastructure
- Enables unit testing with mock repositories

### Service Layer Pattern
- Encapsulates business logic
- Orchestrates operations across multiple repositories
- Returns standardized `AppResult<T>` objects

### Command Query Responsibility Segregation (CQRS)
- **Commands**: Modify state (ClaimQuest, CompleteQuest)
- **Queries**: Read state (GetAvailableQuests, GetLeaderboard)
- Clear separation of read and write operations

### Dependency Injection
- Constructor injection throughout
- All services and repositories registered in DI container
- Promotes loose coupling and testability

### Unit of Work Pattern
- DbContext serves as Unit of Work
- Transactional consistency across repository operations

---

## Security & Identity

### Authentication
- ASP.NET Core Identity for user management
- Cookie-based authentication
- Password requirements: minimum 6 characters (relaxed for gamification focus)

### Authorization
- Role-based authorization (Player, DungeonMaster)
- Controller action authorization via `[Authorize(Roles = "...")]` attributes
- Party-scoped data access (users only see their party's data)

---

## Database Schema Highlights

### Key Relationships
- **User → Party**: Many-to-One (users belong to one party)
- **Party → Quests**: One-to-Many (party has many quests)
- **Quest → QuestCompletions**: One-to-Many (quest can be completed multiple times)
- **User → QuestCompletions**: One-to-Many (user has completion history)
- **QuestCompletion → LootDrop**: One-to-One (optional loot from completion)
- **User → LootDrops**: One-to-Many (user collects loot)

### Constraints & Indexes
- Unique constraints on usernames and party invite codes
- Foreign key relationships with appropriate delete behaviors (Cascade, Restrict)
- Timestamps for creation and update tracking

---

## Testing Strategy

### Unit Testing
- **Target**: ChoreWars.Core services
- **Framework**: xUnit
- **Approach**: Mock repositories and dependencies using interfaces
- **Coverage**: Business logic, progression calculations, loot generation

### Integration Testing
- Database integration tests with in-memory or test PostgreSQL instance
- Controller integration tests for end-to-end workflows

---

## Deployment

### Docker Containerization
- **Dockerfile**: Multi-stage build for optimized image size
- **docker-compose.yml**: Production orchestration (app + PostgreSQL)
- **docker-compose.dev.yml**: Development environment with volume mounts

### Database Migrations
- Automatic migration on startup via `context.Database.Migrate()`
- Migration files tracked in version control
- Rollback support via EF Core CLI

---

## Future Enhancements

Based on project roadmap files:
- **Boss Battles**: Collaborative party-wide challenges
- **Advanced Loot System**: Rarity tiers, equipment slots, stat bonuses
- **Scheduling**: Automated quest recurrence (daily/weekly reset)
- **Notifications**: Real-time alerts for quest assignments and completions
- **Analytics Dashboard**: Insights into party performance and trends

---

## Architecture Benefits

### Achieved Goals
✅ **Testability**: Core business logic is fully unit testable without infrastructure  
✅ **Maintainability**: Clear separation of concerns across layers  
✅ **Scalability**: Easy to add new features without affecting existing code  
✅ **Flexibility**: Can swap out infrastructure (e.g., change database) without touching Core  
✅ **Domain Focus**: Business rules are centralized and explicit in Core layer  

---

## References

- Clean Architecture principles by Robert C. Martin (Uncle Bob)
- Clean Architecture implementation by Steve Smith (Ardalis)
- ASP.NET Core documentation
- Entity Framework Core documentation
- PostgreSQL best practices

---

**Last Updated**: January 2, 2026  
**Version**: 2.0 (Post-PostgreSQL Migration)
