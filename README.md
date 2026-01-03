# ChoreWars 🏰⚔️

> Transform household chores into an epic RPG adventure! Level up, earn gold, and become the ultimate champion of your household.

ChoreWars is a gamified chore management system that turns mundane household tasks into exciting quests. Complete chores to earn experience points, level up your character, collect epic loot, and compete with your household members on the leaderboard!

[![.NET](https://img.shields.io/badge/.NET-10.0-purple.svg)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![Docker](https://img.shields.io/badge/docker-ready-brightgreen.svg)](DOCKER.md)

---

## ✨ Features

### 🎮 RPG Mechanics
- **Level System**: Earn XP and level up with exponential progression
- **Character Stats**: Build your character with Strength, Intelligence, and Constitution
- **Loot Drops**: 20% chance to find epic items when completing quests
- **Stat Milestones**: Unlock special achievements at 10, 25, 50, 100, 250, and 500 stats

### 🎯 Quest Management
- **Quest Types**: Daily, Weekly, and One-Time quests
- **Difficulty Levels**: Easy, Medium, and Hard challenges
- **Stat Bonuses**: Quests can grant permanent stat increases
- **Quest Board**: Visual card-based interface for browsing available quests
- **Quest Import**: Bulk import quests via JSON

### 👥 Party System
- **Household Parties**: Create or join a party with your household members
- **Invite Codes**: Simple 6-character codes for easy party joining
- **Party Leaderboard**: Compete with household members
- **Activity Feed**: See what everyone in your party is accomplishing

### 🏆 Leaderboard & Competition
- **Multiple Rankings**: Sort by XP or Gold
- **Visual Indicators**: Gold, Silver, and Bronze gradients for top 3 players
- **Real-time Stats**: View everyone's level, XP, gold, and character stats
- **Personal Highlights**: Your position is highlighted on the leaderboard

### 🎨 User Experience
- **Dark Mode**: Beautiful dark theme by default with toggle support
- **Responsive Design**: Works seamlessly on desktop, tablet, and mobile
- **Intuitive UI**: Card-based layouts with emoji icons for visual appeal
- **Instant Feedback**: Real-time updates and confirmation messages

### 👤 Role-Based Access
- **Players**: Claim and complete quests, view leaderboard, track progress
- **Dungeon Masters (DMs)**: Create quests, verify completions, manage rewards, import quests

---

## 🚀 Quick Start

### Using Docker (Recommended)

```bash
# Clone the repository
git clone <repository-url>
cd chore-wars-next

# Start the application
make up

# Or using docker-compose
docker-compose up -d

# Access at http://localhost:5287
```

For detailed Docker instructions, see [DOCKER.md](DOCKER.md).

### Manual Setup

#### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQLite (included with .NET)
- A modern web browser

#### Installation

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd chore-wars-next
   ```

2. **Restore dependencies**
   ```bash
   cd src/ChoreWars.Web
   dotnet restore
   ```

3. **Run database migrations** (if needed)
   ```bash
   dotnet ef database update
   ```

4. **Run the application**
   ```bash
   dotnet run
   ```

5. **Access the application**
   - Open your browser to `http://localhost:5287`
   - Or `https://localhost:7206` for HTTPS

---

## 📖 User Guide

### Getting Started

1. **Create an Account**
   - Click "Register" and create your account
   - Choose a username and password

2. **Join or Create a Party**
   - Create a new party if you're the first in your household
   - Or join an existing party using an invite code

3. **Explore the Quest Board**
   - View available quests
   - See quest rewards (XP, Gold, Stat bonuses)
   - Check quest difficulty and type

4. **Complete Quests**
   - Claim a quest to start working on it
   - Mark it complete when done
   - Wait for DM verification to receive rewards

5. **Check the Leaderboard**
   - See your ranking among party members
   - View everyone's stats and progress
   - Toggle between XP and Gold rankings

### For Players

#### Claiming Quests
1. Navigate to the Quest Board
2. Browse available quests
3. Click "⚔️ Claim Quest" on the quest you want
4. Quest moves to "My Active Quests"

#### Completing Quests
1. Do the actual chore/task
2. Go to "My Active Quests"
3. Click "✓ Mark Complete"
4. Status changes to "⏳ Awaiting DM Verification"

#### Unclaiming Quests
- If you can't complete a quest, click "✗ Unclaim Quest"
- Quest returns to available pool

#### Viewing Progress
- **Player Stats Card**: See your level, XP progress, gold, and stats
- **Leaderboard**: Compare with party members
- **Activity Feed**: (Coming soon) View party achievements

### For Dungeon Masters

#### Creating Quests
1. Go to "DM Dashboard"
2. Click "Create New Quest"
3. Fill in quest details:
   - Title and description
   - XP and Gold rewards
   - Difficulty (Easy/Medium/Hard)
   - Quest Type (Daily/Weekly/One-Time)
   - Optional stat bonuses (STR/INT/CON)
4. Click "Create Quest"

#### Verifying Completions
1. Go to "DM Dashboard"
2. Click "Pending Verifications"
3. Review completed quests
4. Click "✓ Approve" to award rewards
5. Or "✗ Reject" if not completed properly

#### Managing Quests
- **Edit**: Modify quest details
- **Delete**: Remove quests
- **Activate/Deactivate**: Control quest availability

#### Importing Quests
1. Prepare a JSON file with quest definitions
2. Go to "DM Dashboard" → "Import Quests"
3. Upload your JSON file
4. Preview and confirm import

**Example JSON format:**
```json
[
  {
    "Title": "Take Out Trash",
    "Description": "Take all trash bins to the curb",
    "XPReward": 50,
    "GoldReward": 10,
    "DifficultyLevel": "Easy",
    "QuestType": "Weekly"
  },
  {
    "Title": "Deep Clean Kitchen",
    "Description": "Thorough kitchen cleaning including appliances",
    "XPReward": 200,
    "GoldReward": 50,
    "StrengthBonus": 2,
    "ConstitutionBonus": 1,
    "DifficultyLevel": "Hard",
    "QuestType": "Weekly"
  }
]
```

---

## 🏗️ Project Structure

```
chore-wars-next/
├── src/
│   ├── ChoreWars.Core/              # Domain layer
│   │   ├── Entities/                # Domain entities (User, Quest, Party, etc.)
│   │   ├── Interfaces/              # Repository and service interfaces
│   │   ├── Services/                # Business logic services
│   │   ├── Commands/                # CQRS command objects
│   │   └── Queries/                 # CQRS query objects
│   │
│   ├── ChoreWars.Infrastructure/    # Data access layer
│   │   ├── Data/                    # EF Core DbContext and migrations
│   │   ├── Identity/                # ASP.NET Identity integration
│   │   └── Repositories/            # Data access implementations
│   │
│   └── ChoreWars.Web/               # Presentation layer
│       ├── Controllers/             # MVC controllers
│       ├── Views/                   # Razor views
│       ├── Models/                  # View models
│       └── wwwroot/                 # Static files (CSS, JS, images)
│
├── Dockerfile                       # Production Docker image
├── docker-compose.yml               # Production orchestration
├── docker-compose.dev.yml           # Development orchestration
├── Makefile                         # Convenience commands
├── DOCKER.md                        # Docker documentation
└── README.md                        # This file
```

### Architecture

ChoreWars follows **Clean Architecture** principles:

- **Core**: Domain entities and business rules (no dependencies)
- **Infrastructure**: Data access, external services (depends on Core)
- **Web**: UI and API endpoints (depends on Core and Infrastructure)

This ensures:
- ✅ Testability
- ✅ Maintainability
- ✅ Independence from frameworks
- ✅ Flexibility to change data access or UI

---

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 10.0 MVC
- **Language**: C# 13
- **Database**: SQLite with Entity Framework Core
- **Authentication**: ASP.NET Core Identity
- **Architecture**: Clean Architecture / CQRS patterns

### Frontend
- **Framework**: Razor Pages (server-side rendering)
- **CSS Framework**: Bootstrap 5.3.3
- **JavaScript**: Vanilla JS (for dark mode toggle)
- **Icons**: Emoji-based (no icon library needed)

### DevOps
- **Containerization**: Docker & Docker Compose
- **Database Migrations**: EF Core Migrations
- **Development**: .NET CLI, Hot Reload support

---

## 🔧 Development Guide

### Prerequisites

- .NET 10.0 SDK
- Your favorite IDE (Visual Studio, VS Code, Rider)
- Git
- Docker (optional, for containerized development)

### Setting Up Development Environment

1. **Clone and restore**
   ```bash
   git clone <repository-url>
   cd chore-wars-next
   dotnet restore
   ```

2. **Run in development mode**
   ```bash
   cd src/ChoreWars.Web
   dotnet run
   ```

3. **Enable hot reload**
   ```bash
   dotnet watch run
   ```

### Database Migrations

**Create a new migration:**
```bash
cd src/ChoreWars.Infrastructure
dotnet ef migrations add MigrationName --startup-project ../ChoreWars.Web
```

**Apply migrations:**
```bash
dotnet ef database update --startup-project ../ChoreWars.Web
```

**Remove last migration:**
```bash
dotnet ef migrations remove --startup-project ../ChoreWars.Web
```

### Building for Production

```bash
# Build
dotnet build -c Release

# Publish
dotnet publish -c Release -o ./publish

# Run published app
cd publish
dotnet ChoreWars.Web.dll
```

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true
```

### Code Style

This project follows standard C# coding conventions:
- PascalCase for public members
- camelCase for private fields
- Async suffix for async methods
- Clear, descriptive names over comments

---

## 🎨 Customization

### Theming

**Dark Mode**: Enabled by default, uses Bootstrap 5.3.3's built-in dark mode.

To change the default theme, edit `/wwwroot/js/site.js`:
```javascript
const DEFAULT_THEME = THEME_LIGHT; // Change from THEME_DARK
```

**Custom Gradients**: Edit gradient colors in `/wwwroot/css/site.css`:
```css
:root {
  --gradient-gold-start: #FFD700;
  --gradient-gold-end: #FFA500;
  /* ... etc */
}
```

### Leveling System

To adjust XP requirements, modify `ProgressionService.cs`:
```csharp
// Current: 100 * 1.5^(level-1)
// Exponential scaling for level progression
```

### Loot Drop Rate

Change loot drop chance in `QuestService.cs`:
```csharp
// Current: 20% chance
var random = new Random();
if (random.Next(100) < 20) // Adjust this number
```

---

## 🐛 Troubleshooting

### Common Issues

**Port already in use:**
```bash
# Change port in Properties/launchSettings.json
# Or use environment variable
ASPNETCORE_URLS=http://localhost:5000 dotnet run
```

**Database locked:**
```bash
# Stop all running instances
# Delete chorewars.db-wal and chorewars.db-shm files
rm chorewars.db-wal chorewars.db-shm
```

**Dark mode not persisting:**
- Check browser's localStorage (F12 → Application → Local Storage)
- Clear cache and reload

**Quest not appearing:**
- Check if quest is marked as "Active"
- Verify quest hasn't been claimed by another user
- Check if you're viewing the right party

---

## 🚢 Deployment

### Docker Deployment (Recommended)

See [DOCKER.md](DOCKER.md) for comprehensive Docker deployment guide.

Quick deploy:
```bash
docker-compose up -d
```

### Manual Deployment

1. **Build for production:**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Copy files to server:**
   ```bash
   scp -r ./publish user@server:/opt/chorewars/
   ```

3. **Set up systemd service** (Linux):
   ```ini
   [Unit]
   Description=ChoreWars Web Application

   [Service]
   WorkingDirectory=/opt/chorewars
   ExecStart=/usr/bin/dotnet /opt/chorewars/ChoreWars.Web.dll
   Restart=always
   RestartSec=10
   SyslogIdentifier=chorewars
   User=www-data
   Environment=ASPNETCORE_ENVIRONMENT=Production

   [Install]
   WantedBy=multi-user.target
   ```

4. **Enable and start:**
   ```bash
   sudo systemctl enable chorewars
   sudo systemctl start chorewars
   ```

### Reverse Proxy (Nginx)

```nginx
server {
    listen 80;
    server_name chorewars.example.com;

    location / {
        proxy_pass http://localhost:5287;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

---

## 🤝 Contributing

Contributions are welcome! Here's how you can help:

1. **Fork the repository**
2. **Create a feature branch** (`git checkout -b feature/AmazingFeature`)
3. **Commit your changes** (`git commit -m 'Add some AmazingFeature'`)
4. **Push to the branch** (`git push origin feature/AmazingFeature`)
5. **Open a Pull Request**

### Development Guidelines

- Follow existing code style and conventions
- Write clear commit messages
- Add tests for new features
- Update documentation as needed
- Ensure all tests pass before submitting PR

---

## 📋 Roadmap

### Planned Features

- [ ] **Rewards System**: Spend gold on real-world rewards
- [ ] **Boss Battles**: Monthly household challenges
- [ ] **Achievements**: Unlock badges and titles
- [ ] **Quest Templates**: Pre-built quest libraries
- [ ] **Mobile App**: Native iOS/Android apps
- [ ] **Notifications**: Push notifications for quest updates
- [ ] **Analytics**: Dashboard with charts and insights
- [ ] **Social Features**: Share achievements, compete with other parties
- [ ] **Customization**: Avatar customization, party themes
- [ ] **API**: RESTful API for integrations

### Recently Completed

- [x] Dark mode theme
- [x] Party leaderboard with rankings
- [x] Quest import functionality
- [x] Loot drop system
- [x] Stat milestone achievements
- [x] Activity feed service
- [x] Docker containerization

---

## 🙏 Acknowledgments

Built with:
- [ASP.NET Core](https://dotnet.microsoft.com/) - Web framework
- [Bootstrap](https://getbootstrap.com/) - UI components
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) - ORM
- [SQLite](https://www.sqlite.org/) - Database
- [Docker](https://www.docker.com/) - Containerization

Inspired by:
- HabitRPG/Habitica
- Epic Win
- Classic RPG progression systems

---

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

## 📞 Support

- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)
- **Email**: support@chorewars.example.com

---

## 🎯 Pro Tips

1. **Set Clear Quest Descriptions**: Be specific about what "clean kitchen" means
2. **Balance Rewards**: Make rewards proportional to effort
3. **Use Stat Bonuses**: Reward consistency with permanent stat increases
4. **Regular DM Check-ins**: Verify quests promptly to keep engagement high
5. **Celebrate Milestones**: Acknowledge level-ups and achievements
6. **Adjust Difficulty**: Fine-tune XP/Gold based on your household
7. **Import in Bulk**: Use JSON import for recurring quests

---

## 🌟 Star History

If you find ChoreWars useful, please consider starring the repository!

[![Star History](https://img.shields.io/github/stars/username/chore-wars-next?style=social)](../../stargazers)

---

<div align="center">

**Made with ❤️ by developers who hate doing chores**

*May your XP be high and your chores be swift!* ⚔️

</div>



### How do you create the first migration?

```
cd /workspaces/chore-wars-next/src/ChoreWars.Web && dotnet ef migrations add InitialCreate --project ../ChoreWars.Infrastructure --context ApplicationDbContext
```

