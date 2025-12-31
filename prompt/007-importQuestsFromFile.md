As a dungeon master over quests, I should be able to import many quests from a JSON file.

- json format should be strongly related to the following csharp class.
- import program should require the following fileds for quest import
    - Title
    - Description
    - XPReward
    - GoldReward
    - DifficultyLevel
- all other fields need to set to sensible defaults or appropriate values
- Make sure to assign a new Guid to the Id of each quest
- Make JSON simple and easy to define via JSON schema
- Quest import screen should document the JSON schema format used for the JSON file.

```
namespace ChoreWars.Core.Entities;

public class Quest
{
    public Guid Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public int XPReward { get; set; }
    public int GoldReward { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public QuestType QuestType { get; set; }

    // Stat Bonuses (Optional - can be 0 if quest doesn't grant stats)
    public int StrengthBonus { get; set; } = 0;
    public int IntelligenceBonus { get; set; } = 0;
    public int ConstitutionBonus { get; set; } = 0;

    public Guid PartyId { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid CreatedByDMId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}
```