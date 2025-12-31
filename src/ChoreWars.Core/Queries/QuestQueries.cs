namespace ChoreWars.Core.Queries;

public class GetAvailableQuestsQuery
{
    public required Guid UserId { get; set; }
    public required Guid PartyId { get; set; }
}

public class GetMyActiveQuestsQuery
{
    public required Guid UserId { get; set; }
}
