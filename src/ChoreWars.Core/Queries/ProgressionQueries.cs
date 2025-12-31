namespace ChoreWars.Core.Queries;

public class GetUserProgressQuery
{
    public required Guid UserId { get; set; }
}

public class GetUserStatsQuery
{
    public required Guid UserId { get; set; }
}
