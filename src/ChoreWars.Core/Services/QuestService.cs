using ChoreWars.Core.Commands;
using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Core.Queries;
using ChoreWars.Core.Results;

namespace ChoreWars.Core.Services;

public class QuestService : IQuestService
{
    private readonly IQuestRepository _questRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRepository<QuestCompletion> _completionRepository;
    private readonly IProgressionService _progressionService;
    private readonly IActivityFeedService _activityFeedService;
    private readonly ILootDropService _lootDropService;

    public QuestService(
        IQuestRepository questRepository,
        IUserRepository userRepository,
        IRepository<QuestCompletion> completionRepository,
        IProgressionService progressionService,
        IActivityFeedService activityFeedService,
        ILootDropService lootDropService)
    {
        _questRepository = questRepository;
        _userRepository = userRepository;
        _completionRepository = completionRepository;
        _progressionService = progressionService;
        _activityFeedService = activityFeedService;
        _lootDropService = lootDropService;
    }

    public async Task<AppResult<QuestCompletionDto>> ClaimQuestAsync(ClaimQuestCommand command)
    {
        // Validate quest exists
        var quest = await _questRepository.GetByIdAsync(command.QuestId);
        if (quest == null)
        {
            return AppResult<QuestCompletionDto>.Failure("Quest not found");
        }

        if (!quest.IsActive)
        {
            return AppResult<QuestCompletionDto>.Failure("Quest is not active");
        }

        // Check if quest is already claimed
        var existingClaim = await _completionRepository.FindAsync(qc =>
            qc.QuestId == command.QuestId &&
            qc.Status == CompletionStatus.Claimed);

        if (existingClaim.Any())
        {
            return AppResult<QuestCompletionDto>.Failure("Quest is already claimed by another user");
        }

        // Check user exists
        var user = await _userRepository.GetByIdAsync(command.UserId);
        if (user == null)
        {
            return AppResult<QuestCompletionDto>.Failure("User not found");
        }

        // Verify user belongs to the quest's party
        if (user.PartyId != quest.PartyId)
        {
            return AppResult<QuestCompletionDto>.Failure("User does not belong to this quest's party");
        }

        // Create quest completion
        var completion = new QuestCompletion
        {
            Id = Guid.NewGuid(),
            QuestId = command.QuestId,
            UserId = command.UserId,
            ClaimedAt = DateTime.UtcNow,
            Status = CompletionStatus.Claimed,
            XPEarned = quest.XPReward,
            GoldEarned = quest.GoldReward,
            StrengthGained = quest.StrengthBonus,
            IntelligenceGained = quest.IntelligenceBonus,
            ConstitutionGained = quest.ConstitutionBonus
        };

        await _completionRepository.AddAsync(completion);
        await _completionRepository.SaveChangesAsync();

        var dto = new QuestCompletionDto
        {
            Id = completion.Id,
            QuestId = quest.Id,
            QuestTitle = quest.Title,
            UserId = user.Id,
            Username = user.Username,
            Status = completion.Status.ToString(),
            XPEarned = completion.XPEarned,
            GoldEarned = completion.GoldEarned,
            StrengthGained = completion.StrengthGained,
            IntelligenceGained = completion.IntelligenceGained,
            ConstitutionGained = completion.ConstitutionGained,
            ClaimedAt = completion.ClaimedAt
        };

        return AppResult<QuestCompletionDto>.Success(dto, "Quest claimed successfully");
    }

    public async Task<AppResult<QuestCompletionDto>> CompleteQuestAsync(CompleteQuestCommand command)
    {
        var completion = await _completionRepository.GetByIdAsync(command.QuestCompletionId);
        if (completion == null)
        {
            return AppResult<QuestCompletionDto>.Failure("Quest completion not found");
        }

        if (completion.UserId != command.UserId)
        {
            return AppResult<QuestCompletionDto>.Failure("User is not the owner of this quest completion");
        }

        if (completion.Status != CompletionStatus.Claimed)
        {
            return AppResult<QuestCompletionDto>.Failure("Quest is not in claimed status");
        }

        completion.Status = CompletionStatus.PendingVerification;
        completion.CompletedAt = DateTime.UtcNow;

        await _completionRepository.UpdateAsync(completion);
        await _completionRepository.SaveChangesAsync();

        var quest = await _questRepository.GetByIdAsync(completion.QuestId);
        var user = await _userRepository.GetByIdAsync(completion.UserId);

        var dto = new QuestCompletionDto
        {
            Id = completion.Id,
            QuestId = completion.QuestId,
            QuestTitle = quest?.Title ?? "",
            UserId = completion.UserId,
            Username = user?.Username ?? "",
            Status = completion.Status.ToString(),
            XPEarned = completion.XPEarned,
            GoldEarned = completion.GoldEarned,
            StrengthGained = completion.StrengthGained,
            IntelligenceGained = completion.IntelligenceGained,
            ConstitutionGained = completion.ConstitutionGained,
            ClaimedAt = completion.ClaimedAt,
            CompletedAt = completion.CompletedAt
        };

        return AppResult<QuestCompletionDto>.Success(dto, "Quest marked as complete, awaiting DM verification");
    }

    public async Task<AppResult<QuestCompletionDto>> UnclaimQuestAsync(UnclaimQuestCommand command)
    {
        // 1. Validate completion exists
        var completion = await _completionRepository.GetByIdAsync(command.QuestCompletionId);
        if (completion == null)
        {
            return AppResult<QuestCompletionDto>.Failure("Quest completion not found");
        }

        // 2. Validate ownership
        if (completion.UserId != command.UserId)
        {
            return AppResult<QuestCompletionDto>.Failure("User is not the owner of this quest completion");
        }

        // 3. Validate status (only Claimed can be unclaimed)
        if (completion.Status != CompletionStatus.Claimed)
        {
            return AppResult<QuestCompletionDto>.Failure("Only quests with 'Claimed' status can be unclaimed");
        }

        // 4. Get related data for DTO before deletion
        var quest = await _questRepository.GetByIdAsync(completion.QuestId);
        var user = await _userRepository.GetByIdAsync(completion.UserId);

        // 5. Create DTO
        var dto = new QuestCompletionDto
        {
            Id = completion.Id,
            QuestId = completion.QuestId,
            QuestTitle = quest?.Title ?? "",
            UserId = completion.UserId,
            Username = user?.Username ?? "",
            Status = "Unclaimed",
            XPEarned = completion.XPEarned,
            GoldEarned = completion.GoldEarned,
            StrengthGained = completion.StrengthGained,
            IntelligenceGained = completion.IntelligenceGained,
            ConstitutionGained = completion.ConstitutionGained,
            ClaimedAt = completion.ClaimedAt
        };

        // 6. Delete the record
        await _completionRepository.DeleteAsync(completion);
        await _completionRepository.SaveChangesAsync();

        return AppResult<QuestCompletionDto>.Success(dto, "Quest unclaimed successfully");
    }

    public async Task<AppResult<QuestCompletionDto>> VerifyQuestAsync(VerifyQuestCommand command)
    {
        var completion = await _completionRepository.GetByIdAsync(command.QuestCompletionId);
        if (completion == null)
        {
            return AppResult<QuestCompletionDto>.Failure("Quest completion not found");
        }

        if (completion.Status != CompletionStatus.PendingVerification)
        {
            return AppResult<QuestCompletionDto>.Failure("Quest is not pending verification");
        }

        // Get quest to verify DM
        var quest = await _questRepository.GetByIdAsync(completion.QuestId);
        if (quest == null)
        {
            return AppResult<QuestCompletionDto>.Failure("Quest not found");
        }

        // Verify DM belongs to the party
        var dm = await _userRepository.GetByIdAsync(command.DMId);
        if (dm == null || dm.PartyId != quest.PartyId)
        {
            return AppResult<QuestCompletionDto>.Failure("Invalid DM for this party");
        }

        if (command.IsApproved)
        {
            completion.Status = CompletionStatus.Approved;
            completion.VerifiedAt = DateTime.UtcNow;
            completion.VerifiedByDMId = command.DMId;

            var completionUser = await _userRepository.GetByIdAsync(completion.UserId);
            if (completionUser == null)
            {
                return AppResult<QuestCompletionDto>.Failure("User not found");
            }

            var oldStats = new
            {
                Strength = completionUser.Strength,
                Intelligence = completionUser.Intelligence,
                Constitution = completionUser.Constitution
            };

            // Award XP, Gold, and Stats
            await _progressionService.AwardXPAsync(new AwardXPCommand
            {
                UserId = completion.UserId,
                XPAmount = completion.XPEarned
            });

            await _progressionService.AwardGoldAsync(new AwardGoldCommand
            {
                UserId = completion.UserId,
                GoldAmount = completion.GoldEarned
            });

            await _progressionService.AwardStatsAsync(new AwardStatsCommand
            {
                UserId = completion.UserId,
                StrengthBonus = completion.StrengthGained,
                IntelligenceBonus = completion.IntelligenceGained,
                ConstitutionBonus = completion.ConstitutionGained
            });

            // Check for level up
            var levelUpResult = await _progressionService.CheckLevelUpAsync(new CheckLevelUpCommand
            {
                UserId = completion.UserId
            });

            // Refresh user to get updated stats and level
            completionUser = await _userRepository.GetByIdAsync(completion.UserId);
            if (completionUser == null)
            {
                return AppResult<QuestCompletionDto>.Failure("User not found after awards");
            }

            // Create activity feed item for quest completion
            await _activityFeedService.CreateQuestCompletedActivityAsync(
                completionUser.Id,
                completionUser.PartyId,
                quest.Title,
                completion.XPEarned,
                completion.GoldEarned,
                completion.StrengthGained,
                completion.IntelligenceGained,
                completion.ConstitutionGained);

            // Create level up activity if level increased
            if (levelUpResult.IsSuccess && levelUpResult.Data != null)
            {
                await _activityFeedService.CreateLevelUpActivityAsync(
                    completionUser.Id,
                    completionUser.PartyId,
                    levelUpResult.Data.NewLevel);
            }

            // Try to generate loot drop (20% chance)
            var lootResult = await _lootDropService.TryGenerateLootDropAsync(
                completionUser.Id,
                completion.Id);

            // Create activity feed item if loot was found
            if (lootResult.IsSuccess && lootResult.Data != null)
            {
                await _activityFeedService.CreateLootFoundActivityAsync(
                    completionUser.Id,
                    completionUser.PartyId,
                    lootResult.Data.Name);
            }

            // Check for stat milestones
            await CheckAndCreateStatMilestones(completionUser, oldStats.Strength, oldStats.Intelligence, oldStats.Constitution);
        }
        else
        {
            completion.Status = CompletionStatus.Rejected;
            completion.VerifiedAt = DateTime.UtcNow;
            completion.VerifiedByDMId = command.DMId;
        }

        await _completionRepository.UpdateAsync(completion);
        await _completionRepository.SaveChangesAsync();

        var user = await _userRepository.GetByIdAsync(completion.UserId);

        var dto = new QuestCompletionDto
        {
            Id = completion.Id,
            QuestId = completion.QuestId,
            QuestTitle = quest.Title,
            UserId = completion.UserId,
            Username = user?.Username ?? "",
            Status = completion.Status.ToString(),
            XPEarned = completion.XPEarned,
            GoldEarned = completion.GoldEarned,
            StrengthGained = completion.StrengthGained,
            IntelligenceGained = completion.IntelligenceGained,
            ConstitutionGained = completion.ConstitutionGained,
            ClaimedAt = completion.ClaimedAt,
            CompletedAt = completion.CompletedAt,
            VerifiedAt = completion.VerifiedAt
        };

        return AppResult<QuestCompletionDto>.Success(dto,
            command.IsApproved ? "Quest verified and rewards granted" : "Quest rejected");
    }

    public async Task<AppResult<List<QuestDto>>> GetAvailableQuestsAsync(GetAvailableQuestsQuery query)
    {
        var quests = await _questRepository.GetAvailableQuestsForUserAsync(query.UserId, query.PartyId);

        var questDtos = quests.Select(q => new QuestDto
        {
            Id = q.Id,
            Title = q.Title,
            Description = q.Description,
            XPReward = q.XPReward,
            GoldReward = q.GoldReward,
            Difficulty = q.Difficulty.ToString(),
            QuestType = q.QuestType.ToString(),
            StrengthBonus = q.StrengthBonus,
            IntelligenceBonus = q.IntelligenceBonus,
            ConstitutionBonus = q.ConstitutionBonus,
            IsClaimed = false
        }).ToList();

        return AppResult<List<QuestDto>>.Success(questDtos);
    }

    public async Task<AppResult<List<QuestDto>>> GetMyActiveQuestsAsync(GetMyActiveQuestsQuery query)
    {
        var completions = await _completionRepository.FindAsync(qc =>
            qc.UserId == query.UserId &&
            (qc.Status == CompletionStatus.Claimed || qc.Status == CompletionStatus.PendingVerification));

        var questDtos = new List<QuestDto>();
        foreach (var completion in completions)
        {
            var quest = await _questRepository.GetByIdAsync(completion.QuestId);
            if (quest != null)
            {
                questDtos.Add(new QuestDto
                {
                    Id = quest.Id,
                    Title = quest.Title,
                    Description = quest.Description,
                    XPReward = quest.XPReward,
                    GoldReward = quest.GoldReward,
                    Difficulty = quest.Difficulty.ToString(),
                    QuestType = quest.QuestType.ToString(),
                    StrengthBonus = quest.StrengthBonus,
                    IntelligenceBonus = quest.IntelligenceBonus,
                    ConstitutionBonus = quest.ConstitutionBonus,
                    IsClaimed = true,
                    CompletionId = completion.Id,
                    CompletionStatus = completion.Status.ToString()
                });
            }
        }

        return AppResult<List<QuestDto>>.Success(questDtos);
    }

    private async Task CheckAndCreateStatMilestones(User user, int oldStrength, int oldIntelligence, int oldConstitution)
    {
        var milestones = new[] { 10, 25, 50, 100, 250, 500 };

        // Check Strength milestones
        foreach (var milestone in milestones)
        {
            if (user.Strength >= milestone && oldStrength < milestone)
            {
                var title = GetStatMilestoneTitle("Strength", milestone);
                await _activityFeedService.CreateStatMilestoneActivityAsync(
                    user.Id,
                    user.PartyId,
                    "Strength",
                    user.Strength,
                    title);
                break; // Only announce one milestone per stat per completion
            }
        }

        // Check Intelligence milestones
        foreach (var milestone in milestones)
        {
            if (user.Intelligence >= milestone && oldIntelligence < milestone)
            {
                var title = GetStatMilestoneTitle("Intelligence", milestone);
                await _activityFeedService.CreateStatMilestoneActivityAsync(
                    user.Id,
                    user.PartyId,
                    "Intelligence",
                    user.Intelligence,
                    title);
                break;
            }
        }

        // Check Constitution milestones
        foreach (var milestone in milestones)
        {
            if (user.Constitution >= milestone && oldConstitution < milestone)
            {
                var title = GetStatMilestoneTitle("Constitution", milestone);
                await _activityFeedService.CreateStatMilestoneActivityAsync(
                    user.Id,
                    user.PartyId,
                    "Constitution",
                    user.Constitution,
                    title);
                break;
            }
        }
    }

    private static string GetStatMilestoneTitle(string statName, int milestone)
    {
        return (statName, milestone) switch
        {
            ("Strength", 10) => "Budding Warrior",
            ("Strength", 25) => "Strong Helper",
            ("Strength", 50) => "Household Warrior",
            ("Strength", 100) => "Physical Powerhouse",
            ("Strength", 250) => "Legendary Hero",
            ("Strength", 500) => "Titan of Chores",

            ("Intelligence", 10) => "Quick Thinker",
            ("Intelligence", 25) => "Smart Planner",
            ("Intelligence", 50) => "Master Planner",
            ("Intelligence", 100) => "Household Genius",
            ("Intelligence", 250) => "Strategic Mastermind",
            ("Intelligence", 500) => "Oracle of Organization",

            ("Constitution", 10) => "Routine Builder",
            ("Constitution", 25) => "Steady Performer",
            ("Constitution", 50) => "Chore Marathoner",
            ("Constitution", 100) => "Endurance Champion",
            ("Constitution", 250) => "Unstoppable Force",
            ("Constitution", 500) => "Immortal Maintainer",

            _ => $"{statName} Master"
        };
    }
}
