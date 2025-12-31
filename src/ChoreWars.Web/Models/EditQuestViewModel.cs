using System.ComponentModel.DataAnnotations;
using ChoreWars.Core.Entities;

namespace ChoreWars.Web.Models;

public class EditQuestViewModel
{
    public Guid Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(1, 1000)]
    [Display(Name = "XP Reward")]
    public int XPReward { get; set; }

    [Required]
    [Range(1, 1000)]
    [Display(Name = "Gold Reward")]
    public int GoldReward { get; set; }

    [Required]
    public DifficultyLevel Difficulty { get; set; }

    [Required]
    [Display(Name = "Quest Type")]
    public QuestType QuestType { get; set; }

    [Display(Name = "Strength Bonus")]
    [Range(0, 10)]
    public int StrengthBonus { get; set; }

    [Display(Name = "Intelligence Bonus")]
    [Range(0, 10)]
    public int IntelligenceBonus { get; set; }

    [Display(Name = "Constitution Bonus")]
    [Range(0, 10)]
    public int ConstitutionBonus { get; set; }

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}
