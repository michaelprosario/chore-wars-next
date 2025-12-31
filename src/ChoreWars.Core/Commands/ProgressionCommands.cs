namespace ChoreWars.Core.Commands;

public class AwardXPCommand
{
    public required Guid UserId { get; set; }
    public required int XPAmount { get; set; }
}

public class AwardGoldCommand
{
    public required Guid UserId { get; set; }
    public required int GoldAmount { get; set; }
}

public class AwardStatsCommand
{
    public required Guid UserId { get; set; }
    public int StrengthBonus { get; set; }
    public int IntelligenceBonus { get; set; }
    public int ConstitutionBonus { get; set; }
}

public class CheckLevelUpCommand
{
    public required Guid UserId { get; set; }
}
