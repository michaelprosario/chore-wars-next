-- ============================================
-- ChoreWars Database Initial Deployment Script
-- PostgreSQL 12+
-- Created: January 3, 2026
-- ============================================

-- Create database (uncomment if needed)
-- CREATE DATABASE chorewars;
-- \c chorewars;

-- ============================================
-- 1. ASP.NET Core Identity Tables
-- ============================================

-- Roles table
CREATE TABLE "AspNetRoles" (
    "Id" TEXT NOT NULL,
    "Name" VARCHAR(256) NULL,
    "NormalizedName" VARCHAR(256) NULL,
    "ConcurrencyStamp" TEXT NULL,
    CONSTRAINT "PK_AspNetRoles" PRIMARY KEY ("Id")
);

-- Users table (Identity)
CREATE TABLE "AspNetUsers" (
    "Id" TEXT NOT NULL,
    "DomainUserId" UUID NULL,
    "UserName" VARCHAR(256) NULL,
    "NormalizedUserName" VARCHAR(256) NULL,
    "Email" VARCHAR(256) NULL,
    "NormalizedEmail" VARCHAR(256) NULL,
    "EmailConfirmed" BOOLEAN NOT NULL,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL,
    "TwoFactorEnabled" BOOLEAN NOT NULL,
    "LockoutEnd" TIMESTAMPTZ NULL,
    "LockoutEnabled" BOOLEAN NOT NULL,
    "AccessFailedCount" INTEGER NOT NULL,
    CONSTRAINT "PK_AspNetUsers" PRIMARY KEY ("Id")
);

-- Role claims
CREATE TABLE "AspNetRoleClaims" (
    "Id" SERIAL NOT NULL,
    "RoleId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "PK_AspNetRoleClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles_RoleId" 
        FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") 
        ON DELETE CASCADE
);

-- User claims
CREATE TABLE "AspNetUserClaims" (
    "Id" SERIAL NOT NULL,
    "UserId" TEXT NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "PK_AspNetUserClaims" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") 
        ON DELETE CASCADE
);

-- User logins
CREATE TABLE "AspNetUserLogins" (
    "LoginProvider" TEXT NOT NULL,
    "ProviderKey" TEXT NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") 
        ON DELETE CASCADE
);

-- User roles
CREATE TABLE "AspNetUserRoles" (
    "UserId" TEXT NOT NULL,
    "RoleId" TEXT NOT NULL,
    CONSTRAINT "PK_AspNetUserRoles" PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles_RoleId" 
        FOREIGN KEY ("RoleId") REFERENCES "AspNetRoles" ("Id") 
        ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") 
        ON DELETE CASCADE
);

-- User tokens
CREATE TABLE "AspNetUserTokens" (
    "UserId" TEXT NOT NULL,
    "LoginProvider" TEXT NOT NULL,
    "Name" TEXT NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "PK_AspNetUserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") 
        ON DELETE CASCADE
);

-- ============================================
-- 2. ChoreWars Core Tables
-- ============================================

-- Parties table
CREATE TABLE "Parties" (
    "Id" UUID NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "InviteCode" VARCHAR(20) NOT NULL,
    "DungeonMasterId" UUID NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL,
    CONSTRAINT "PK_Parties" PRIMARY KEY ("Id")
);

-- Users table (Domain)
CREATE TABLE "Users" (
    "Id" UUID NOT NULL,
    "Username" VARCHAR(50) NOT NULL,
    "DisplayName" VARCHAR(100) NOT NULL,
    "CurrentLevel" INTEGER NOT NULL,
    "CurrentXP" INTEGER NOT NULL,
    "XPToNextLevel" INTEGER NOT NULL,
    "TotalGold" INTEGER NOT NULL,
    "AvatarUrl" TEXT NULL,
    "PartyId" UUID NOT NULL,
    "Strength" INTEGER NOT NULL,
    "Intelligence" INTEGER NOT NULL,
    "Constitution" INTEGER NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL,
    "LastActiveAt" TIMESTAMPTZ NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Users_Parties_PartyId" 
        FOREIGN KEY ("PartyId") REFERENCES "Parties" ("Id") 
        ON DELETE RESTRICT
);

-- Quests table
CREATE TABLE "Quests" (
    "Id" UUID NOT NULL,
    "Title" VARCHAR(200) NOT NULL,
    "Description" TEXT NOT NULL,
    "XPReward" INTEGER NOT NULL,
    "GoldReward" INTEGER NOT NULL,
    "Difficulty" INTEGER NOT NULL,
    "QuestType" INTEGER NOT NULL,
    "StrengthBonus" INTEGER NOT NULL,
    "IntelligenceBonus" INTEGER NOT NULL,
    "ConstitutionBonus" INTEGER NOT NULL,
    "PartyId" UUID NOT NULL,
    "IsActive" BOOLEAN NOT NULL,
    "CreatedByDMId" UUID NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL,
    CONSTRAINT "PK_Quests" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Quests_Parties_PartyId" 
        FOREIGN KEY ("PartyId") REFERENCES "Parties" ("Id") 
        ON DELETE CASCADE
);

-- Quest completions table
CREATE TABLE "QuestCompletions" (
    "Id" UUID NOT NULL,
    "QuestId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "ClaimedAt" TIMESTAMPTZ NOT NULL,
    "CompletedAt" TIMESTAMPTZ NULL,
    "VerifiedAt" TIMESTAMPTZ NULL,
    "VerifiedByDMId" UUID NULL,
    "Status" INTEGER NOT NULL,
    "XPEarned" INTEGER NOT NULL,
    "GoldEarned" INTEGER NOT NULL,
    "StrengthGained" INTEGER NOT NULL,
    "IntelligenceGained" INTEGER NOT NULL,
    "ConstitutionGained" INTEGER NOT NULL,
    CONSTRAINT "PK_QuestCompletions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_QuestCompletions_Quests_QuestId" 
        FOREIGN KEY ("QuestId") REFERENCES "Quests" ("Id") 
        ON DELETE CASCADE,
    CONSTRAINT "FK_QuestCompletions_Users_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") 
        ON DELETE CASCADE
);

-- Loot drops table
CREATE TABLE "LootDrops" (
    "Id" UUID NOT NULL,
    "Name" VARCHAR(100) NOT NULL,
    "Description" TEXT NOT NULL,
    "Rarity" INTEGER NOT NULL,
    "IconUrl" TEXT NULL,
    "UserId" UUID NOT NULL,
    "QuestCompletionId" UUID NOT NULL,
    "FoundAt" TIMESTAMPTZ NOT NULL,
    CONSTRAINT "PK_LootDrops" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_LootDrops_QuestCompletions_QuestCompletionId" 
        FOREIGN KEY ("QuestCompletionId") REFERENCES "QuestCompletions" ("Id") 
        ON DELETE CASCADE,
    CONSTRAINT "FK_LootDrops_Users_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") 
        ON DELETE RESTRICT
);

-- Rewards table
CREATE TABLE "Rewards" (
    "Id" UUID NOT NULL,
    "Title" VARCHAR(200) NOT NULL,
    "Description" TEXT NOT NULL,
    "GoldCost" INTEGER NOT NULL,
    "PartyId" UUID NOT NULL,
    "IsAvailable" BOOLEAN NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL,
    CONSTRAINT "PK_Rewards" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Rewards_Parties_PartyId" 
        FOREIGN KEY ("PartyId") REFERENCES "Parties" ("Id") 
        ON DELETE CASCADE
);

-- Reward purchases table
CREATE TABLE "RewardPurchases" (
    "Id" UUID NOT NULL,
    "RewardId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "GoldSpent" INTEGER NOT NULL,
    "RequestedAt" TIMESTAMPTZ NOT NULL,
    "FulfilledAt" TIMESTAMPTZ NULL,
    "FulfilledByDMId" UUID NULL,
    "Status" INTEGER NOT NULL,
    "Memo" TEXT NULL,
    CONSTRAINT "PK_RewardPurchases" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RewardPurchases_Rewards_RewardId" 
        FOREIGN KEY ("RewardId") REFERENCES "Rewards" ("Id") 
        ON DELETE CASCADE,
    CONSTRAINT "FK_RewardPurchases_Users_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") 
        ON DELETE CASCADE
);

-- Activity feed items table
CREATE TABLE "ActivityFeedItems" (
    "Id" UUID NOT NULL,
    "PartyId" UUID NOT NULL,
    "UserId" UUID NOT NULL,
    "ActivityType" INTEGER NOT NULL,
    "Message" TEXT NOT NULL,
    "CreatedAt" TIMESTAMPTZ NOT NULL,
    "Metadata" TEXT NULL,
    CONSTRAINT "PK_ActivityFeedItems" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ActivityFeedItems_Parties_PartyId" 
        FOREIGN KEY ("PartyId") REFERENCES "Parties" ("Id") 
        ON DELETE CASCADE,
    CONSTRAINT "FK_ActivityFeedItems_Users_UserId" 
        FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") 
        ON DELETE CASCADE
);

-- Boss battles table
CREATE TABLE "BossBattles" (
    "Id" UUID NOT NULL,
    "Name" VARCHAR(200) NOT NULL,
    "Description" TEXT NOT NULL,
    "RequiredTotalXP" INTEGER NOT NULL,
    "CurrentXP" INTEGER NOT NULL,
    "PartyId" UUID NOT NULL,
    "GroupRewardDescription" TEXT NOT NULL,
    "IsActive" BOOLEAN NOT NULL,
    "StartedAt" TIMESTAMPTZ NOT NULL,
    "CompletedAt" TIMESTAMPTZ NULL,
    CONSTRAINT "PK_BossBattles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_BossBattles_Parties_PartyId" 
        FOREIGN KEY ("PartyId") REFERENCES "Parties" ("Id") 
        ON DELETE CASCADE
);

-- ============================================
-- 3. Indexes
-- ============================================

-- Identity indexes
CREATE UNIQUE INDEX "RoleNameIndex" ON "AspNetRoles" ("NormalizedName");
CREATE INDEX "IX_AspNetRoleClaims_RoleId" ON "AspNetRoleClaims" ("RoleId");
CREATE INDEX "IX_AspNetUserClaims_UserId" ON "AspNetUserClaims" ("UserId");
CREATE INDEX "IX_AspNetUserLogins_UserId" ON "AspNetUserLogins" ("UserId");
CREATE INDEX "IX_AspNetUserRoles_RoleId" ON "AspNetUserRoles" ("RoleId");
CREATE INDEX "EmailIndex" ON "AspNetUsers" ("NormalizedEmail");
CREATE UNIQUE INDEX "UserNameIndex" ON "AspNetUsers" ("NormalizedUserName");

-- ChoreWars indexes
CREATE UNIQUE INDEX "IX_Parties_InviteCode" ON "Parties" ("InviteCode");
CREATE INDEX "IX_Users_PartyId" ON "Users" ("PartyId");
CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");
CREATE INDEX "IX_Quests_PartyId" ON "Quests" ("PartyId");
CREATE INDEX "IX_QuestCompletions_QuestId" ON "QuestCompletions" ("QuestId");
CREATE INDEX "IX_QuestCompletions_UserId" ON "QuestCompletions" ("UserId");
CREATE UNIQUE INDEX "IX_LootDrops_QuestCompletionId" ON "LootDrops" ("QuestCompletionId");
CREATE INDEX "IX_LootDrops_UserId" ON "LootDrops" ("UserId");
CREATE INDEX "IX_Rewards_PartyId" ON "Rewards" ("PartyId");
CREATE INDEX "IX_RewardPurchases_RewardId" ON "RewardPurchases" ("RewardId");
CREATE INDEX "IX_RewardPurchases_UserId" ON "RewardPurchases" ("UserId");
CREATE INDEX "IX_ActivityFeedItems_PartyId_CreatedAt" ON "ActivityFeedItems" ("PartyId", "CreatedAt");
CREATE INDEX "IX_ActivityFeedItems_UserId" ON "ActivityFeedItems" ("UserId");
CREATE INDEX "IX_BossBattles_PartyId" ON "BossBattles" ("PartyId");

-- ============================================
-- 4. Migration History Table
-- ============================================

CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" VARCHAR(150) NOT NULL,
    "ProductVersion" VARCHAR(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

-- Insert migration record
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260103094106_InitialCreate', '9.0.0');

-- ============================================
-- 5. Comments (Documentation)
-- ============================================

COMMENT ON TABLE "Parties" IS 'Groups/families that participate in ChoreWars together';
COMMENT ON TABLE "Users" IS 'Domain users representing party members with RPG stats';
COMMENT ON TABLE "Quests" IS 'Tasks/chores that can be completed for rewards';
COMMENT ON TABLE "QuestCompletions" IS 'Records of quest completion with verification workflow';
COMMENT ON TABLE "LootDrops" IS 'Random items earned from completing quests';
COMMENT ON TABLE "Rewards" IS 'Real-world rewards that can be purchased with gold';
COMMENT ON TABLE "RewardPurchases" IS 'Records of reward redemption requests';
COMMENT ON TABLE "ActivityFeedItems" IS 'Activity log for party members';
COMMENT ON TABLE "BossBattles" IS 'Collaborative challenges for parties';

-- ============================================
-- Deployment Complete
-- ============================================
