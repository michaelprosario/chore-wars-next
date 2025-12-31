using ChoreWars.Core.Commands;
using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Core.Queries;
using ChoreWars.Core.Services;
using NSubstitute;
using NUnit.Framework;

namespace ChoreWars.Core.Tests.Services;

[TestFixture]
public class ProgressionServiceTests
{
    private IUserRepository _userRepository = null!;
    private ProgressionService _service = null!;
    private User _testUser = null!;

    [SetUp]
    public void Setup()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _service = new ProgressionService(_userRepository);

        _testUser = new User
        {
            Id = Guid.NewGuid(),
            Username = "testuser",
            DisplayName = "Test User",
            CurrentLevel = 1,
            CurrentXP = 0,
            XPToNextLevel = 100,
            TotalGold = 0,
            PartyId = Guid.NewGuid(),
            Strength = 0,
            Intelligence = 0,
            Constitution = 0
        };
    }

    [Test]
    public async Task AwardXPAsync_UserExists_AwardsXPSuccessfully()
    {
        // Arrange
        var command = new AwardXPCommand
        {
            UserId = _testUser.Id,
            XPAmount = 50
        };
        _userRepository.GetByIdAsync(_testUser.Id).Returns(_testUser);

        // Act
        var result = await _service.AwardXPAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.CurrentXP, Is.EqualTo(50));
        await _userRepository.Received(1).UpdateAsync(_testUser);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task AwardXPAsync_UserNotFound_ReturnsFailure()
    {
        // Arrange
        var command = new AwardXPCommand
        {
            UserId = Guid.NewGuid(),
            XPAmount = 50
        };
        _userRepository.GetByIdAsync(command.UserId).Returns((User?)null);

        // Act
        var result = await _service.AwardXPAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Errors, Contains.Item("User not found"));
    }

    [Test]
    public async Task AwardGoldAsync_UserExists_AwardsGoldSuccessfully()
    {
        // Arrange
        var command = new AwardGoldCommand
        {
            UserId = _testUser.Id,
            GoldAmount = 25
        };
        _userRepository.GetByIdAsync(_testUser.Id).Returns(_testUser);

        // Act
        var result = await _service.AwardGoldAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.TotalGold, Is.EqualTo(25));
        await _userRepository.Received(1).UpdateAsync(_testUser);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task AwardStatsAsync_UserExists_AwardsStatsSuccessfully()
    {
        // Arrange
        var command = new AwardStatsCommand
        {
            UserId = _testUser.Id,
            StrengthBonus = 2,
            IntelligenceBonus = 1,
            ConstitutionBonus = 3
        };
        _userRepository.GetByIdAsync(_testUser.Id).Returns(_testUser);

        // Act
        var result = await _service.AwardStatsAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Strength, Is.EqualTo(2));
        Assert.That(result.Data.Intelligence, Is.EqualTo(1));
        Assert.That(result.Data.Constitution, Is.EqualTo(3));
        await _userRepository.Received(1).UpdateAsync(_testUser);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task CheckLevelUpAsync_SufficientXP_LevelsUp()
    {
        // Arrange
        _testUser.CurrentXP = 100; // Exactly enough to level up
        var command = new CheckLevelUpCommand
        {
            UserId = _testUser.Id
        };
        _userRepository.GetByIdAsync(_testUser.Id).Returns(_testUser);

        // Act
        var result = await _service.CheckLevelUpAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.NewLevel, Is.EqualTo(2));
        Assert.That(result.Data.OldLevel, Is.EqualTo(1));
        await _userRepository.Received(1).UpdateAsync(_testUser);
        await _userRepository.Received(1).SaveChangesAsync();
    }

    [Test]
    public async Task CheckLevelUpAsync_InsufficientXP_DoesNotLevelUp()
    {
        // Arrange
        _testUser.CurrentXP = 50; // Not enough to level up
        var command = new CheckLevelUpCommand
        {
            UserId = _testUser.Id
        };
        _userRepository.GetByIdAsync(_testUser.Id).Returns(_testUser);

        // Act
        var result = await _service.CheckLevelUpAsync(command);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Null);
        await _userRepository.DidNotReceive().UpdateAsync(Arg.Any<User>());
    }

    [Test]
    public async Task GetUserProgressAsync_UserExists_ReturnsProgress()
    {
        // Arrange
        var query = new GetUserProgressQuery
        {
            UserId = _testUser.Id
        };
        _userRepository.GetByIdAsync(_testUser.Id).Returns(_testUser);

        // Act
        var result = await _service.GetUserProgressAsync(query);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.UserId, Is.EqualTo(_testUser.Id));
        Assert.That(result.Data.Username, Is.EqualTo(_testUser.Username));
    }

    [Test]
    public async Task GetUserStatsAsync_UserExists_ReturnsStats()
    {
        // Arrange
        _testUser.Strength = 10;
        _testUser.Intelligence = 15;
        _testUser.Constitution = 20;
        var query = new GetUserStatsQuery
        {
            UserId = _testUser.Id
        };
        _userRepository.GetUserWithStatsAsync(_testUser.Id).Returns(_testUser);

        // Act
        var result = await _service.GetUserStatsAsync(query);

        // Assert
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Data, Is.Not.Null);
        Assert.That(result.Data!.Strength, Is.EqualTo(10));
        Assert.That(result.Data.Intelligence, Is.EqualTo(15));
        Assert.That(result.Data.Constitution, Is.EqualTo(20));
    }
}
