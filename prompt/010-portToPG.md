# Port ChoreWars from SQLite to PostgreSQL

## Overview
Port the ChoreWars application from SQLite to PostgreSQL to provide a more robust, scalable database solution suitable for production deployment. PostgreSQL offers better concurrency, data integrity, and performance for web applications with multiple users.

## Current State Analysis

### SQLite Implementation
- **Database Provider**: Microsoft.EntityFrameworkCore.Sqlite v10.0.1
- **Connection String**: `Data Source=chorewars.db` (or `/app/data/chorewars.db` in Docker)
- **Location**: 
  - Development: Local file in Web project directory
  - Docker: Mounted volume at `/app/data/chorewars.db`
- **Migrations**: Two existing migrations (InitialCreate, AddIdentity)

### Impacted Components
1. **ChoreWars.Infrastructure** - Database provider and configuration
2. **ChoreWars.Web** - Connection string and startup configuration
3. **Docker Configuration** - docker-compose.yml and docker-compose.dev.yml
4. **Migrations** - May need regeneration for PostgreSQL-specific syntax

## Implementation Plan

### Phase 1: Infrastructure Changes

#### 1.1 Update NuGet Packages
**File**: `src/ChoreWars.Infrastructure/ChoreWars.Infrastructure.csproj`

**Changes**:
- Remove: `Microsoft.EntityFrameworkCore.Sqlite` (10.0.1)
- Add: `Npgsql.EntityFrameworkCore.PostgreSQL` (10.0.0 or latest)

**Reasoning**: Npgsql is the official PostgreSQL provider for Entity Framework Core.

#### 1.2 Update ApplicationDbContext (if needed)
**File**: `src/ChoreWars.Infrastructure/Data/ApplicationDbContext.cs`

**Review Items**:
- Check for SQLite-specific configurations (unlikely, current code is provider-agnostic)
- Case sensitivity: PostgreSQL is case-sensitive by default
- String comparisons: May need to update query patterns
- Data types: PostgreSQL has different handling for some types (e.g., GUIDs, booleans)

**Note**: Current configuration looks provider-agnostic and should work without changes.

### Phase 2: Web Application Changes

#### 2.1 Update Program.cs
**File**: `src/ChoreWars.Web/Program.cs`

**Change**:
```csharp
// FROM:
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? "Data Source=chorewars.db"));

// TO:
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));
```

**Reasoning**: 
- Replace `UseSqlite` with `UseNpgsql`
- Remove file-based fallback (PostgreSQL requires server connection)
- Throw explicit error if connection string is missing

#### 2.2 Update Configuration Files
**Files**: 
- `src/ChoreWars.Web/appsettings.json`
- `src/ChoreWars.Web/appsettings.Development.json`

**Add ConnectionStrings section**:

**appsettings.json** (Production/Default):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=postgres;Database=chorewars;Username=chorewars;Password=change_this_password_in_production"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**appsettings.Development.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=chorewars_dev;Username=chorewars_dev;Password=dev_password;Port=5432"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### Phase 3: Database Migrations

#### 3.1 Regenerate Migrations
**Actions**:
1. **Backup existing migrations** (optional - for reference)
2. **Delete existing migrations** in `src/ChoreWars.Infrastructure/Migrations/`
3. **Create new initial migration** for PostgreSQL:
   ```bash
   cd src/ChoreWars.Infrastructure
   dotnet ef migrations add InitialCreatePostgres --startup-project ../ChoreWars.Web
   ```

**Reasoning**: While EF Core migrations often work across providers, PostgreSQL may generate different SQL for:
- Identity columns (SERIAL vs AUTOINCREMENT)
- Boolean types (bool vs INTEGER)
- Case-sensitive table/column names
- Index creation syntax

**Alternative Approach**: Keep existing migrations if they work, only regenerate if issues occur.

### Phase 4: Docker Configuration

#### 4.1 Development PostgreSQL Setup (Separate Container)
**File**: `pgDockerCompose/docker-compose.yml` *(Already exists)*

**Current setup** - Standalone PostgreSQL with pgAdmin for development:
```yaml
services:
  postgres:
    container_name: postgres_container
    image: postgres
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-postgres}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-Foobar321}
      PGDATA: /data/postgres
    volumes:
       - postgres:/data/postgres
    ports:
      - "5432:5432"
    networks:
      - postgres
    restart: unless-stopped
  
  pgadmin:
    container_name: pgadmin_container
    image: dpage/pgadmin4
    environment:
      PGADMIN_DEFAULT_EMAIL: ${PGADMIN_DEFAULT_EMAIL:-pgadmin4@pgadmin.org}
      PGADMIN_DEFAULT_PASSWORD: ${PGADMIN_DEFAULT_PASSWORD:-Foobar321}
    volumes:
       - pgadmin:/root/.pgadmin
    ports:
      - "${PGADMIN_PORT:-5050}:80"
    networks:
      - postgres
    restart: unless-stopped

networks:
  postgres:
    external: true

volumes:
    postgres:
    pgadmin:
```

**Usage for Development**:
```bash
# Start PostgreSQL + pgAdmin
cd pgDockerCompose
docker-compose up -d

# Access pgAdmin at http://localhost:5050
# Login: pgadmin4@pgadmin.org / Foobar321

# Stop PostgreSQL
docker-compose down
```

**Connection Details for Local Development**:
- Host: `localhost`
- Port: `5432`
- User: `postgres` (default)
- Password: `Foobar321` (default)
- Database: `chorewars` (needs to be created)

**Initial Database Setup**:
```bash
# Create the database (one-time setup)
docker exec -it postgres_container psql -U postgres -c "CREATE DATABASE chorewars;"
```

#### 4.2 Update Development Configuration
**File**: `src/ChoreWars.Web/appsettings.Development.json`

**Updated connection string** to use standalone PostgreSQL:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=chorewars;Username=postgres;Password=Foobar321;Port=5432"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

**Note**: This connects to the PostgreSQL instance running in `pgDockerCompose`, accessible on localhost:5432.

#### 4.3 Update Production Docker Compose
**File**: `docker-compose.yml`

**Add PostgreSQL service for production**:
```yaml
services:
  postgres:
    image: postgres:17-alpine
    container_name: chorewars-postgres
    restart: unless-stopped
    environment:
      POSTGRES_DB: chorewars
      POSTGRES_USER: chorewars
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-change_this_password}
    volumes:
      - postgres-data:/var/lib/postgresql/data
    networks:
      - chorewars-network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U chorewars"]
      interval: 10s
      timeout: 5s
      retries: 5

  chorewars-web:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: chorewars-web
    restart: unless-stopped
    ports:
      - "5287:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=chorewars;Username=chorewars;Password=${POSTGRES_PASSWORD:-change_this_password}
    depends_on:
      postgres:
        condition: service_healthy
    networks:
      - chorewars-network
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
      interval: 30s
      timeout: 3s
      start_period: 40s
      retries: 3
    labels:
      - "com.chorewars.description=ChoreWars Web Application"
      - "com.chorewars.version=1.0"

volumes:
  postgres-data:
    driver: local

networks:
  chorewars-network:
    driver: bridge
```

**Changes Summary**:
- Add PostgreSQL 17 Alpine service (lightweight)
- Remove SQLite volume mounts
- Add `postgres-data` volume for database persistence
- Update connection string to point to postgres service
- Add `depends_on` with health check to ensure database is ready
- Use environment variable for password security
- No port exposure (internal network only for security)

#### 4.4 Update Development Docker Compose (Optional)
**File**: `docker-compose.dev.yml`

**Option 1** - Continue using standalone PostgreSQL in `pgDockerCompose` (Recommended):
- Keep existing docker-compose.dev.yml as-is or remove it
- Developers run PostgreSQL separately via `pgDockerCompose/docker-compose.yml`
- Application connects to localhost:5432
- Benefit: Can access pgAdmin for debugging

**Option 2** - Integrate PostgreSQL into dev compose:
```yaml
version: '3.8'

services:
  chorewars-web:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: chorewars-web-dev
    restart: unless-stopped
    ports:
      - "5287:8080"
    volumes:
      - ./src/ChoreWars.Web:/app/src/ChoreWars.Web:ro
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8080
      - ConnectionStrings__DefaultConnection=Host=host.docker.internal;Database=chorewars;Username=postgres;Password=Foobar321;Port=5432
      - Logging__LogLevel__Default=Debug
      - Logging__LogLevel__Microsoft.AspNetCore=Warning
      - Logging__LogLevel__Microsoft.EntityFrameworkCore.Database.Command=Information
    extra_hosts:
      - "host.docker.internal:host-gateway"
    networks:
      - chorewars-network

networks:
  chorewars-network:
    driver: bridge
```

**Note**: Uses `host.docker.internal` to connect to PostgreSQL running on host machine.

#### 4.5 Create Environment File Template
**File**: `.env.example`

**Create new file**:
```bash
# PostgreSQL Configuration (for production docker-compose.yml)
POSTGRES_PASSWORD=change_this_password_in_production
POSTGRES_USER=chorewars
POSTGRES_DB=chorewars

# Application Configuration
ASPNETCORE_ENVIRONMENT=Production
```

**Add to .gitignore** (if not already present):
```
.env
```

**File**: `pgDockerCompose/.env.example`

**Create new file** for dev PostgreSQL:
```bash
# PostgreSQL Development Configuration
POSTGRES_USER=postgres
POSTGRES_PASSWORD=Foobar321

# pgAdmin Configuration
PGADMIN_DEFAULT_EMAIL=pgadmin4@pgadmin.org
PGADMIN_DEFAULT_PASSWORD=Foobar321
PGADMIN_PORT=5050
```

### Phase 5: Documentation Updates

#### 5.1 Update DOCKER.md
**File**: `DOCKER.md`

**Add sections**:
- PostgreSQL service description for production
- Separate development setup using `pgDockerCompose/`
- Connection string format
- Database initialization steps
- How to access PostgreSQL directly (psql)
- How to use pgAdmin for development
- Backup and restore procedures
- Environment variables for security

**Development workflow**:
```bash
# 1. Start PostgreSQL + pgAdmin (one terminal)
cd pgDockerCompose
docker-compose up -d

# 2. Create database (first time only)
docker exec -it postgres_container psql -U postgres -c "CREATE DATABASE chorewars;"

# 3. Run application locally
cd ..
dotnet run --project src/ChoreWars.Web

# Or run application in Docker (optional)
docker-compose -f docker-compose.dev.yml up
```

#### 5.2 Update README.md
**File**: `README.md`

**Update sections**:
- Prerequisites:
  - For local development: PostgreSQL (via Docker recommended)
  - Run `pgDockerCompose/docker-compose.yml` for dev database
- Database section (change from SQLite to PostgreSQL)
- Getting started steps:
  ```bash
  # 1. Start PostgreSQL
  cd pgDockerCompose && docker-compose up -d && cd ..
  
  # 2. Create database
  docker exec -it postgres_container psql -U postgres -c "CREATE DATABASE chorewars;"
  
  # 3. Run migrations
  dotnet ef database update --project src/ChoreWars.Infrastructure --startup-project src/ChoreWars.Web
  
  # 4. Run application
  dotnet run --project src/ChoreWars.Web
  ```
- Connection string examples
- pgAdmin access: http://localhost:5050

#### 5.3 Create Migration Guide
**File**: `MIGRATION.md` (new)

**Content**:
- Steps for migrating existing SQLite data to PostgreSQL
- Data export/import procedures
- Rollback strategy

### Phase 6: Testing and Validation

#### 6.1 Local Development Testing
**Steps**:
1. Start PostgreSQL: `cd pgDockerCompose && docker-compose up -d`
2. Create database: `docker exec -it postgres_container psql -U postgres -c "CREATE DATABASE chorewars;"`
3. Verify connection in appsettings.Development.json points to localhost:5432
4. Run migrations: `dotnet ef database update --project src/ChoreWars.Infrastructure --startup-project src/ChoreWars.Web`
5. Start application: `dotnet run --project src/ChoreWars.Web`
6. Test application functionality:
   - User registration/login
   - Party creation
   - Quest operations
   - Loot drops
   - Reward system
   - Activity feed
7. Verify data in pgAdmin (http://localhost:5050)

#### 6.2 Docker Testing
**Steps**:
1. Build images: `docker-compose build`
2. Start services: `docker-compose up -d`
3. Check PostgreSQL health: `docker-compose ps`
4. Verify migrations ran: Check logs `docker-compose logs chorewars-web`
5. Test application functionality
6. Verify data persistence: Stop and restart containers

#### 6.3 Performance Testing
**Areas to test**:
- Query performance (should be similar or better)
- Concurrent user operations
- Large dataset handling
- Connection pooling behavior

## Potential Issues and Solutions

### Issue 1: Case Sensitivity
**Problem**: PostgreSQL is case-sensitive for table/column names if quoted
**Solution**: Use EF Core naming conventions consistently (PascalCase)
**Mitigation**: Test all queries, especially raw SQL if any

### Issue 2: Boolean Type Handling
**Problem**: SQLite stores booleans as integers (0/1), PostgreSQL has native bool type
**Solution**: EF Core handles this automatically
**Mitigation**: Test all boolean conditions in queries

### Issue 3: Date/Time Handling
**Problem**: Different datetime storage and timezone handling
**Solution**: Use UTC consistently, let EF Core handle conversions
**Mitigation**: Review datetime queries and storage

### Issue 4: GUID Generation
**Problem**: PostgreSQL handles UUIDs differently than SQLite
**Solution**: Verify GUID/UUID generation in ApplicationUser (Identity)
**Mitigation**: Test user registration and Identity functionality

### Issue 5: Migration Compatibility
**Problem**: Existing SQLite migrations may not work with PostgreSQL
**Solution**: Regenerate migrations for PostgreSQL
**Mitigation**: Keep SQLite migrations for reference, create new PostgreSQL branch

### Issue 6: Connection String Security
**Problem**: Hardcoded passwords in configuration files
**Solution**: Use environment variables and .env files (not committed)
**Mitigation**: Add .env.example template, document in README

## Rollback Strategy

If issues arise during PostgreSQL migration:

1. **Immediate rollback**:
   - Revert code changes using git
   - Restore SQLite docker-compose configuration
   - Use existing SQLite database file

2. **Data preservation**:
   - Export PostgreSQL data before rollback
   - Keep PostgreSQL volume for analysis
   - Document issues encountered

3. **Parallel operation** (alternative):
   - Maintain SQLite configuration on a branch
   - Test PostgreSQL thoroughly before full cutover
   - Support both providers via configuration flag

## Success Criteria

- ✅ Application runs successfully with PostgreSQL
- ✅ All existing functionality works (user auth, quests, parties, rewards)
- ✅ Database migrations apply cleanly
- ✅ Docker Compose setup works for both dev and prod
- ✅ Data persists across container restarts
- ✅ Performance is acceptable (same or better than SQLite)
- ✅ Documentation is updated and accurate
- ✅ No hardcoded passwords in repository

## Implementation Order

1. **Setup (Low Risk)**:
   - Create .env.example (root and pgDockerCompose/)
   - Update .gitignore
   - Verify pgDockerCompose/docker-compose.yml is running
   - Create chorewars database in PostgreSQL
   - Update documentation

2. **Infrastructure (Medium Risk)**:
   - Update ChoreWars.Infrastructure.csproj
   - Review ApplicationDbContext

3. **Web Application (Medium Risk)**:
   - Update Program.cs
   - Update appsettings.json files (production connection)
   - Update appsettings.Development.json (localhost:5432 connection)

4. **Docker (Can Test Independently)**:
   - Update docker-compose.yml (production with embedded PostgreSQL)
   - Update or simplify docker-compose.dev.yml (optional)

5. **Migrations (Can Regenerate)**:
   - Backup old migrations
   - Create new PostgreSQL migrations

6. **Testing (Critical)**:
   - Local development testing (using pgDockerCompose)
   - Docker testing (production setup)
   - Full application regression testing

## Estimated Effort

- **Code Changes**: 2-3 hours
- **Migration Creation**: 30 minutes
- **Docker Configuration**: 1 hour
- **Testing**: 2-3 hours
- **Documentation**: 1 hour
- **Total**: 6-8 hours

## Future Enhancements

After successful PostgreSQL migration:

1. **Database optimization**:
   - Add database indexes for common queries
   - Implement connection pooling configuration
   - Configure PostgreSQL for optimal performance

2. **Advanced features**:
   - Use PostgreSQL-specific features (JSONB columns, full-text search)
   - Implement database-level constraints
   - Add database triggers for complex logic

3. **Monitoring**:
   - Add database health checks
   - Implement query logging
   - Setup performance monitoring

4. **Backup strategy**:
   - Automated PostgreSQL backups
   - Point-in-time recovery configuration
   - Backup verification procedures
