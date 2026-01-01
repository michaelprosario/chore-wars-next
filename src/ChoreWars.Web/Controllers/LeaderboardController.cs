using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChoreWars.Web.Controllers;

[Authorize]
public class LeaderboardController : Controller
{
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public LeaderboardController(
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
    }

    private async Task<User?> GetCurrentDomainUserAsync()
    {
        var identityUser = await _userManager.GetUserAsync(User);
        if (identityUser?.DomainUserId == null) return null;

        return await _userRepository.GetByIdAsync(identityUser.DomainUserId.Value);
    }

    public async Task<IActionResult> Index(string sortBy = "xp")
    {
        var currentUser = await GetCurrentDomainUserAsync();
        if (currentUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        IEnumerable<User> leaderboardUsers;

        if (sortBy?.ToLower() == "gold")
        {
            leaderboardUsers = await _userRepository.GetGoldLeaderboardAsync(currentUser.PartyId);
            ViewBag.SortBy = "gold";
        }
        else
        {
            leaderboardUsers = await _userRepository.GetWeeklyLeaderboardAsync(currentUser.PartyId);
            ViewBag.SortBy = "xp";
        }

        ViewBag.CurrentUser = currentUser;

        return View(leaderboardUsers);
    }
}
