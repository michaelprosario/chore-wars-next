using System.ComponentModel.DataAnnotations;

namespace ChoreWars.Web.Models;

public class RegisterViewModel
{
    [Required]
    [Display(Name = "Username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Confirm password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Display(Name = "Party Invite Code (optional - leave blank to create new party)")]
    public string? InviteCode { get; set; }

    [Display(Name = "Register as Dungeon Master (Party Leader)")]
    public bool IsDungeonMaster { get; set; }

    [Display(Name = "Party Name (required if Dungeon Master)")]
    public string? PartyName { get; set; }
}
