using ChoreWars.Core.Commands;
using ChoreWars.Core.Interfaces;
using ChoreWars.Core.Queries;
using ChoreWars.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChoreWars.Web.Controllers;

[Authorize]
public class QuestController : Controller
{
    private readonly IQuestService _questService;
    private readonly IUserRepository _userRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public QuestController(
        IQuestService questService,
        IUserRepository userRepository,
        UserManager<ApplicationUser> userManager)
    {
        _questService = questService;
        _userRepository = userRepository;
        _userManager = userManager;
    }

    private async Task<Guid?> GetCurrentUserDomainIdAsync()
    {
        var identityUser = await _userManager.GetUserAsync(User);
        return identityUser?.DomainUserId;
    }

    private async Task<Core.Entities.User?> GetCurrentDomainUserAsync()
    {
        var domainUserId = await GetCurrentUserDomainIdAsync();
        if (domainUserId == null) return null;

        return await _userRepository.GetByIdAsync(domainUserId.Value);
    }

    /// <summary>
    /// Quest Board - Shows available quests and user's active quests
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        // Get available quests
        var availableQuestsResult = await _questService.GetAvailableQuestsAsync(
            new GetAvailableQuestsQuery
            {
                UserId = domainUser.Id,
                PartyId = domainUser.PartyId
            });

        // Get user's active quests
        var activeQuestsResult = await _questService.GetMyActiveQuestsAsync(
            new GetMyActiveQuestsQuery
            {
                UserId = domainUser.Id
            });

        ViewBag.AvailableQuests = availableQuestsResult.Data ?? new List<Core.Interfaces.QuestDto>();
        ViewBag.ActiveQuests = activeQuestsResult.Data ?? new List<Core.Interfaces.QuestDto>();
        ViewBag.User = domainUser;

        return View();
    }

    /// <summary>
    /// Claim a quest
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Claim(Guid questId)
    {
        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _questService.ClaimQuestAsync(new ClaimQuestCommand
        {
            QuestId = questId,
            UserId = domainUser.Id
        });

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = result.Message ?? "Quest claimed successfully!";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", result.Errors);
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Complete a quest (mark as pending verification)
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(Guid questCompletionId)
    {
        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _questService.CompleteQuestAsync(new CompleteQuestCommand
        {
            QuestCompletionId = questCompletionId,
            UserId = domainUser.Id
        });

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = result.Message ?? "Quest completed! Awaiting DM verification.";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", result.Errors);
        }

        return RedirectToAction(nameof(Index));
    }
}
