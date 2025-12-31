using ChoreWars.Core.Commands;
using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Core.Queries;
using ChoreWars.Core.Services;
using NSubstitute;
using NUnit.Framework;

namespace ChoreWars.Core.Tests.Services;

[TestFixture]
public class QuestServiceTests
{
    private IQuestRepository _questRepository = null!;
    private IUserRepository _userRepository = null!;
    private IRepository<QuestCompletion> _completionRepository = null!;
    private IProgressionService _progressionService = null!;
    private IActivityFeedService _activityFeedService = null!;
    private ILootDropService _lootDropService = null!;
    private QuestService _service = null!;
    private User _testUser = null!;
    private Quest _testQuest = null!;
    private Guid _partyId;

    [SetUp]
    public void Setup()
    {
        _questRepository = Substitute.For<IQuestRepository>();
        _userRepository = Substitute.For<IUserRepository>();
        _completionRepository = Substitute.For<IRepository<QuestCompletion>>();
        _progressionService = Substitute.For<IProgressionService>();
        _activityFeedService = Substitute.For<IActivityFeedService>();
        _lootDropService = Substitute.For<ILootDropService>();
        _service = new QuestService(_questRepository, _userRepository, _completionRepository, _progressionService, _activityFeedService, _lootDropService);

        _partyId = Guid.NewGuid();
        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            DisplayName = "Test User",
            PartyId = _partyId,
            CurrentLevel = 1,
            CurrentXP = 0,
            XPToNextLevel = 100,
            TotalGold = 0
        };

        _testQuest = new Quest
        {
            Id = Guid.NewGuid(),
            Title = "Test Quest",
            Description = "Test Description",
            XPReward = 50,
            GoldReward = 25,
            Difficulty = DifficultyLevel.Medium,
            QuestType = QuestType.OneTime,
            StrengthBonus = 2,
            PartyId = _partyId,
            IsActive = true,
            CreatedByDMId = Guid.NewGuid()
        };
    }

    [Test]
    public async Task ClaimQuestAsync_ValidQuest_ClaimsSuccessfully()
    {
        // Arrange
        var command = new ClaimQuestCommand
        {
            QuestId = _testQuest.Id,
            UserId = _testUser.Id
        };
        _questRepository.GetByIdAsync(_testQuest.Id).Returns(_testQuest);
        _userRepository.GetByIdAsync(_testUser.Id).Returns(_testUser);
        _completionRepository.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<QuestCompletion, bool>>>())
            .Returns(new List<QuestCompletion>());

        // Act
        var result = await _service.ClaimQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.XPEarned, Is.EqualTo(50));
        Assert.That(result.Data.GoldEarned, Is.EqualTo(25));
        Assert.That(result.Data.StrengthGained, Is.EqualTo(2));
        await _completionRepository.Received(1).AddAsync(Arg.Any<QuestCompletion>());
        await _completionRepository.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task ClaimQuestAsync_QuestNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new ClaimQuestCommand
        {
            QuestId = Guid.NewGuid(),
            UserId = _testUser.Id
        };
        _questRepository.GetByIdAsync(command.QuestId).Returns((Quest?)null);

        // Act
        var result = await _service.ClaimQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Contains.Item("Quest not found"));
    }

    [Test]
    public async Task ClaimQuestAsync_QuestInactive_ReturnsFailure()
    {
        // Arrange
        _testQuest.IsActive = false;
        var command = new ClaimQuestCommand
        {
            QuestId = _testQuest.Id,
            UserId = _testUser.Id
        };
        _questRepository.GetByIdAsync(_testQuest.Id).Returns(_testQuest);

        // Act
        var result = await _service.ClaimQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Contains.Item("Quest is not active"));
    }

    [Test]
    public async Task ClaimQuestAsync_QuestAlreadyClaimed_ReturnsFailure()
    {
        // Arrange
        var command = new ClaimQuestCommand
        {
            QuestId = _testQuest.Id,
            UserId = _testUser.Id
        };
        var existingClaim = new QuestCompletion
        {
            Id = Guid.NewGuid(),
            QuestId = _testQuest.Id,
            UserId = Guid.NewGuid(),
            Status = CompletionStatus.Claimed,
            XPEarned = 50,
            GoldEarned = 25
        };
        _questRepository.GetByIdAsync(_testQuest.Id).Returns(_testQuest);
        _completionRepository.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<QuestCompletion, bool>>>())
            .Returns(new List<QuestCompletion> { existingClaim });

        // Act
        var result = await _service.ClaimQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Contains.Item("Quest is already claimed by another user"));
    }

    [Test]
    public async Task ClaimQuestAsync_UserNotInParty_ReturnsFailure()
    {
        // Arrange
        var differentPartyUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "otheruser",
            DisplayName = "Other User",
            PartyId = Guid.NewGuid(), // Different party
            CurrentLevel = 1,
            CurrentXP = 0,
            XPToNextLevel = 100,
            TotalGold = 0
        };
        var command = new ClaimQuestCommand
        {
            QuestId = _testQuest.Id,
            UserId = differentPartyUser.Id
        };
        _questRepository.GetByIdAsync(_testQuest.Id).Returns(_testQuest);
        _userRepository.GetByIdAsync(differentPartyUser.Id).Returns(differentPartyUser);
        _completionRepository.FindAsync(Arg.Any<System.Linq.Expressions.Expression<Func<QuestCompletion, bool>>>())
            .Returns(new List<QuestCompletion>());

        // Act
        var result = await _service.ClaimQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Contains.Item("User does not belong to this quest's party"));
    }

    [Test]
    public async Task CompleteQuestAsync_ValidCompletion_MarksAsComplete()
    {
        // Arrange
        var completion = new QuestCompletion
        {
            Id = Guid.NewGuid(),
            QuestId = _testQuest.Id,
            UserId = _testUser.Id,
            Status = CompletionStatus.Claimed,
            XPEarned = 50,
            GoldEarned = 25
        };
        var command = new CompleteQuestCommand
        {
            QuestCompletionId = completion.Id,
            UserId = _testUser.Id
        };
        _completionRepository.GetByIdAsync(completion.Id).Returns(completion);
        _questRepository.GetByIdAsync(_testQuest.Id).Returns(_testQuest);
        _userRepository.GetByIdAsync(_testUser.Id).Returns(_testUser);

        // Act
        var result = await _service.CompleteQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(completion.Status, Is.EqualTo(CompletionStatus.PendingVerification));
        Assert.That(completion.CompletedAt, Is.Not.Null);
        await _completionRepository.Received(1).UpdateAsync(completion);
        await _completionRepository.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task VerifyQuestAsync_ApprovedQuest_AwardsRewards()
    {
        // Arrange
        var dm = new User
        {
            Id = Guid.NewGuid(),
            Username = "dm",
            DisplayName = "Dungeon Master",
            PartyId = _partyId,
            CurrentLevel = 1,
            CurrentXP = 0,
            XPToNextLevel = 100,
            TotalGold = 0
        };
        var completion = new QuestCompletion
        {
            Id = Guid.NewGuid(),
            QuestId = _testQuest.Id,
            UserId = _testUser.Id,
            Status = CompletionStatus.PendingVerification,
            XPEarned = 50,
            GoldEarned = 25,
            StrengthGained = 2
        };
        var command = new VerifyQuestCommand
        {
            QuestCompletionId = completion.Id,
            DMId = dm.Id,
            IsApproved = true
        };
        _completionRepository.GetByIdAsync(completion.Id).Returns(completion);
        _questRepository.GetByIdAsync(_testQuest.Id).Returns(_testQuest);
        _userRepository.GetByIdAsync(dm.Id).Returns(dm);
        _userRepository.GetByIdAsync(_testUser.Id).Returns(_testUser);

        // Mock progression service responses
        _progressionService.AwardXPAsync(Arg.Any<AwardXPCommand>())
            .Returns(Results.AppResult<UserProgressDto>.Success(new UserProgressDto()));
        _progressionService.AwardGoldAsync(Arg.Any<AwardGoldCommand>())
            .Returns(Results.AppResult<UserProgressDto>.Success(new UserProgressDto()));
        _progressionService.AwardStatsAsync(Arg.Any<AwardStatsCommand>())
            .Returns(Results.AppResult<UserProgressDto>.Success(new UserProgressDto()));
        _progressionService.CheckLevelUpAsync(Arg.Any<CheckLevelUpCommand>())
            .Returns(Results.AppResult<LevelUpDto?>.Success(null));

        // Mock activity feed service responses
        _activityFeedService.CreateQuestCompletedActivityAsync(
            Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<string>(),
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>())
            .Returns(Results.AppResult<ActivityFeedItem>.Success(new ActivityFeedItem { Message = "Test" }));

        // Mock loot drop service response
        _lootDropService.TryGenerateLootDropAsync(Arg.Any<Guid>(), Arg.Any<Guid>())
            .Returns(Results.AppResult<LootDrop?>.Success(null));

        // Act
        var result = await _service.VerifyQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(completion.Status, Is.EqualTo(CompletionStatus.Approved));
        Assert.That(completion.VerifiedAt, Is.Not.Null);
        Assert.That(completion.VerifiedByDMId, Is.EqualTo(dm.Id));
        await _progressionService.Received(1).AwardXPAsync(Arg.Any<AwardXPCommand>());
        await _progressionService.Received(1).AwardGoldAsync(Arg.Any<AwardGoldCommand>());
        await _progressionService.Received(1).AwardStatsAsync(Arg.Any<AwardStatsCommand>());
        await _progressionService.Received(1).CheckLevelUpAsync(Arg.Any<CheckLevelUpCommand>());
    }

    [Test]
    public async Task VerifyQuestAsync_RejectedQuest_DoesNotAwardRewards()
    {
        // Arrange
        var dm = new User
        {
            Id = Guid.NewGuid(),
            Username = "dm",
            DisplayName = "Dungeon Master",
            PartyId = _partyId,
            CurrentLevel = 1,
            CurrentXP = 0,
            XPToNextLevel = 100,
            TotalGold = 0
        };
        var completion = new QuestCompletion
        {
            Id = Guid.NewGuid(),
            QuestId = _testQuest.Id,
            UserId = _testUser.Id,
            Status = CompletionStatus.PendingVerification,
            XPEarned = 50,
            GoldEarned = 25
        };
        var command = new VerifyQuestCommand
        {
            QuestCompletionId = completion.Id,
            DMId = dm.Id,
            IsApproved = false
        };
        _completionRepository.GetByIdAsync(completion.Id).Returns(completion);
        _questRepository.GetByIdAsync(_testQuest.Id).Returns(_testQuest);
        _userRepository.GetByIdAsync(dm.Id).Returns(dm);
        _userRepository.GetByIdAsync(_testUser.Id).Returns(_testUser);

        // Act
        var result = await _service.VerifyQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(completion.Status, Is.EqualTo(CompletionStatus.Rejected));
        await _progressionService.DidNotReceive().AwardXPAsync(Arg.Any<AwardXPCommand>());
        await _progressionService.DidNotReceive().AwardGoldAsync(Arg.Any<AwardGoldCommand>());
    }

    [Test]
    public async Task UnclaimQuestAsync_ValidClaim_UnclainsSuccessfully()
    {
        // Arrange
        var completion = new QuestCompletion
        {
            Id = Guid.NewGuid(),
            QuestId = _testQuest.Id,
            UserId = _testUser.Id,
            Status = CompletionStatus.Claimed,
            XPEarned = 50,
            GoldEarned = 25,
            StrengthGained = 2
        };
        var command = new UnclaimQuestCommand
        {
            QuestCompletionId = completion.Id,
            UserId = _testUser.Id
        };
        _completionRepository.GetByIdAsync(completion.Id).Returns(completion);
        _questRepository.GetByIdAsync(_testQuest.Id).Returns(_testQuest);
        _userRepository.GetByIdAsync(_testUser.Id).Returns(_testUser);

        // Act
        var result = await _service.UnclaimQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        await _completionRepository.Received(1).DeleteAsync(completion);
        await _completionRepository.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task UnclaimQuestAsync_CompletionNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new UnclaimQuestCommand
        {
            QuestCompletionId = Guid.NewGuid(),
            UserId = _testUser.Id
        };
        _completionRepository.GetByIdAsync(command.QuestCompletionId).Returns((QuestCompletion?)null);

        // Act
        var result = await _service.UnclaimQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Contains.Item("Quest completion not found"));
    }

    [Test]
    public async Task UnclaimQuestAsync_NotOwner_ReturnsFailure()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        var completion = new QuestCompletion
        {
            Id = Guid.NewGuid(),
            QuestId = _testQuest.Id,
            UserId = otherUserId,
            Status = CompletionStatus.Claimed
        };
        var command = new UnclaimQuestCommand
        {
            QuestCompletionId = completion.Id,
            UserId = _testUser.Id
        };
        _completionRepository.GetByIdAsync(completion.Id).Returns(completion);

        // Act
        var result = await _service.UnclaimQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Contains.Item("User is not the owner of this quest completion"));
    }

    [Test]
    public async Task UnclaimQuestAsync_PendingVerificationStatus_ReturnsFailure()
    {
        // Arrange
        var completion = new QuestCompletion
        {
            Id = Guid.NewGuid(),
            QuestId = _testQuest.Id,
            UserId = _testUser.Id,
            Status = CompletionStatus.PendingVerification
        };
        var command = new UnclaimQuestCommand
        {
            QuestCompletionId = completion.Id,
            UserId = _testUser.Id
        };
        _completionRepository.GetByIdAsync(completion.Id).Returns(completion);

        // Act
        var result = await _service.UnclaimQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Contains.Item("Only quests with 'Claimed' status can be unclaimed"));
    }

    [Test]
    public async Task UnclaimQuestAsync_ApprovedStatus_ReturnsFailure()
    {
        // Arrange
        var completion = new QuestCompletion
        {
            Id = Guid.NewGuid(),
            QuestId = _testQuest.Id,
            UserId = _testUser.Id,
            Status = CompletionStatus.Approved
        };
        var command = new UnclaimQuestCommand
        {
            QuestCompletionId = completion.Id,
            UserId = _testUser.Id
        };
        _completionRepository.GetByIdAsync(completion.Id).Returns(completion);

        // Act
        var result = await _service.UnclaimQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Contains.Item("Only quests with 'Claimed' status can be unclaimed"));
    }

    [Test]
    public async Task UnclaimQuestAsync_RejectedStatus_ReturnsFailure()
    {
        // Arrange
        var completion = new QuestCompletion
        {
            Id = Guid.NewGuid(),
            QuestId = _testQuest.Id,
            UserId = _testUser.Id,
            Status = CompletionStatus.Rejected
        };
        var command = new UnclaimQuestCommand
        {
            QuestCompletionId = completion.Id,
            UserId = _testUser.Id
        };
        _completionRepository.GetByIdAsync(completion.Id).Returns(completion);

        // Act
        var result = await _service.UnclaimQuestAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Contains.Item("Only quests with 'Claimed' status can be unclaimed"));
    }
}
