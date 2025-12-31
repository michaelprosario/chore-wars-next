using System.ComponentModel.DataAnnotations;
using ChoreWars.Core.Entities;

namespace ChoreWars.Web.Models;

public class CreateQuestViewModel
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    [Display(Name = "XP Reward")]
    public int XPReward { get; set; } = 50;

    [Required]
    [Range(1, 1000)]
    [Display(Name = "Gold Reward")]
    public int GoldReward { get; set; } = 10;

    [Required]
    public DifficultyLevel Difficulty { get; set; } = DifficultyLevel.Easy;

    [Required]
    [Display(Name = "Quest Type")]
    public QuestType QuestType { get; set; } = QuestType.OneTime;

    [Display(Name = "Strength Bonus")]
    [Range(0, 10)]
    public int StrengthBonus { get; set; } = 0;

    [Display(Name = "Intelligence Bonus")]
    [Range(0, 10)]
    public int IntelligenceBonus { get; set; } = 0;

    [Display(Name = "Constitution Bonus")]
    [Range(0, 10)]
    public int ConstitutionBonus { get; set; } = 0;
}
