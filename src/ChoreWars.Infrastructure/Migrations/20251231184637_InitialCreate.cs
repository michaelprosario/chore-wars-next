using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChoreWars.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Parties",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    InviteCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    DungeonMasterId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BossBattles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    RequiredTotalXP = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentXP = table.Column<int>(type: "INTEGER", nullable: false),
                    PartyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GroupRewardDescription = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BossBattles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BossBattles_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Quests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    XPReward = table.Column<int>(type: "INTEGER", nullable: false),
                    GoldReward = table.Column<int>(type: "INTEGER", nullable: false),
                    Difficulty = table.Column<int>(type: "INTEGER", nullable: false),
                    QuestType = table.Column<int>(type: "INTEGER", nullable: false),
                    StrengthBonus = table.Column<int>(type: "INTEGER", nullable: false),
                    IntelligenceBonus = table.Column<int>(type: "INTEGER", nullable: false),
                    ConstitutionBonus = table.Column<int>(type: "INTEGER", nullable: false),
                    PartyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedByDMId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Quests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Quests_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rewards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    GoldCost = table.Column<int>(type: "INTEGER", nullable: false),
                    PartyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    IsAvailable = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rewards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rewards_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    CurrentLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    CurrentXP = table.Column<int>(type: "INTEGER", nullable: false),
                    XPToNextLevel = table.Column<int>(type: "INTEGER", nullable: false),
                    TotalGold = table.Column<int>(type: "INTEGER", nullable: false),
                    AvatarUrl = table.Column<string>(type: "TEXT", nullable: true),
                    PartyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Strength = table.Column<int>(type: "INTEGER", nullable: false),
                    Intelligence = table.Column<int>(type: "INTEGER", nullable: false),
                    Constitution = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastActiveAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityFeedItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PartyId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ActivityType = table.Column<int>(type: "INTEGER", nullable: false),
                    Message = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Metadata = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityFeedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ActivityFeedItems_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActivityFeedItems_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "QuestCompletions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClaimedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    VerifiedByDMId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    XPEarned = table.Column<int>(type: "INTEGER", nullable: false),
                    GoldEarned = table.Column<int>(type: "INTEGER", nullable: false),
                    StrengthGained = table.Column<int>(type: "INTEGER", nullable: false),
                    IntelligenceGained = table.Column<int>(type: "INTEGER", nullable: false),
                    ConstitutionGained = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestCompletions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestCompletions_Quests_QuestId",
                        column: x => x.QuestId,
                        principalTable: "Quests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuestCompletions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RewardPurchases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RewardId = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    GoldSpent = table.Column<int>(type: "INTEGER", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FulfilledAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FulfilledByDMId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false),
                    Memo = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RewardPurchases", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RewardPurchases_Rewards_RewardId",
                        column: x => x.RewardId,
                        principalTable: "Rewards",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RewardPurchases_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LootDrops",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Rarity = table.Column<int>(type: "INTEGER", nullable: false),
                    IconUrl = table.Column<string>(type: "TEXT", nullable: true),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    QuestCompletionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    FoundAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LootDrops", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LootDrops_QuestCompletions_QuestCompletionId",
                        column: x => x.QuestCompletionId,
                        principalTable: "QuestCompletions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LootDrops_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityFeedItems_PartyId_CreatedAt",
                table: "ActivityFeedItems",
                columns: new[] { "PartyId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityFeedItems_UserId",
                table: "ActivityFeedItems",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BossBattles_PartyId",
                table: "BossBattles",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_LootDrops_QuestCompletionId",
                table: "LootDrops",
                column: "QuestCompletionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LootDrops_UserId",
                table: "LootDrops",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_InviteCode",
                table: "Parties",
                column: "InviteCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_QuestCompletions_QuestId",
                table: "QuestCompletions",
                column: "QuestId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestCompletions_UserId",
                table: "QuestCompletions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Quests_PartyId",
                table: "Quests",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardPurchases_RewardId",
                table: "RewardPurchases",
                column: "RewardId");

            migrationBuilder.CreateIndex(
                name: "IX_RewardPurchases_UserId",
                table: "RewardPurchases",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rewards_PartyId",
                table: "Rewards",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PartyId",
                table: "Users",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivityFeedItems");

            migrationBuilder.DropTable(
                name: "BossBattles");

            migrationBuilder.DropTable(
                name: "LootDrops");

            migrationBuilder.DropTable(
                name: "RewardPurchases");

            migrationBuilder.DropTable(
                name: "QuestCompletions");

            migrationBuilder.DropTable(
                name: "Rewards");

            migrationBuilder.DropTable(
                name: "Quests");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Parties");
        }
    }
}
