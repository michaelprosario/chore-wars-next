using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Infrastructure.Identity;
using ChoreWars.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChoreWars.Web.Controllers;

[Authorize(Roles = "DungeonMaster")]
public class DMController : Controller
{
    private readonly IQuestRepository _questRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRepository<QuestCompletion> _questCompletionRepository;
    private readonly IRepository<Party> _partyRepository;
    private readonly UserManager<ApplicationUser> _userManager;

    public DMController(
        IQuestRepository questRepository,
        IUserRepository userRepository,
        IRepository<QuestCompletion> questCompletionRepository,
        IRepository<Party> partyRepository,
        UserManager<ApplicationUser> userManager)
    {
        _questRepository = questRepository;
        _userRepository = userRepository;
        _questCompletionRepository = questCompletionRepository;
        _partyRepository = partyRepository;
        _userManager = userManager;
    }

    private async Task<Guid?> GetCurrentUserDomainIdAsync()
    {
        var identityUser = await _userManager.GetUserAsync(User);
        return identityUser?.DomainUserId;
    }

    private async Task<User?> GetCurrentDomainUserAsync()
    {
        var domainUserId = await GetCurrentUserDomainIdAsync();
        if (domainUserId == null) return null;

        return await _userRepository.GetByIdAsync(domainUserId.Value);
    }

    public async Task<IActionResult> Index()
    {
        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var party = await _partyRepository.GetByIdAsync(domainUser.PartyId);
        if (party == null)
        {
            return NotFound("Party not found");
        }

        var quests = await _questRepository.GetByPartyIdAsync(domainUser.PartyId);

        ViewBag.PartyName = party.Name;
        ViewBag.InviteCode = party.InviteCode;

        return View(quests);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateQuestViewModel model)
    {
        if (ModelState.IsValid)
        {
            var domainUser = await GetCurrentDomainUserAsync();
            if (domainUser == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var quest = new Quest
            {
                Id = Guid.NewGuid(),
                Title = model.Title,
                Description = model.Description,
                XPReward = model.XPReward,
                GoldReward = model.GoldReward,
                Difficulty = model.Difficulty,
                QuestType = model.QuestType,
                StrengthBonus = model.StrengthBonus,
                IntelligenceBonus = model.IntelligenceBonus,
                ConstitutionBonus = model.ConstitutionBonus,
                PartyId = domainUser.PartyId,
                CreatedByDMId = domainUser.Id,
                IsActive = true
            };

            await _questRepository.AddAsync(quest);
            await _questRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Quest created successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var quest = await _questRepository.GetByIdAsync(id);
        if (quest == null)
        {
            return NotFound();
        }

        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null || quest.PartyId != domainUser.PartyId)
        {
            return Forbid();
        }

        var model = new EditQuestViewModel
        {
            Id = quest.Id,
            Title = quest.Title,
            Description = quest.Description,
            XPReward = quest.XPReward,
            GoldReward = quest.GoldReward,
            Difficulty = quest.Difficulty,
            QuestType = quest.QuestType,
            StrengthBonus = quest.StrengthBonus,
            IntelligenceBonus = quest.IntelligenceBonus,
            ConstitutionBonus = quest.ConstitutionBonus,
            IsActive = quest.IsActive
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditQuestViewModel model)
    {
        if (ModelState.IsValid)
        {
            var quest = await _questRepository.GetByIdAsync(model.Id);
            if (quest == null)
            {
                return NotFound();
            }

            var domainUser = await GetCurrentDomainUserAsync();
            if (domainUser == null || quest.PartyId != domainUser.PartyId)
            {
                return Forbid();
            }

            quest.Title = model.Title;
            quest.Description = model.Description;
            quest.XPReward = model.XPReward;
            quest.GoldReward = model.GoldReward;
            quest.Difficulty = model.Difficulty;
            quest.QuestType = model.QuestType;
            quest.StrengthBonus = model.StrengthBonus;
            quest.IntelligenceBonus = model.IntelligenceBonus;
            quest.ConstitutionBonus = model.ConstitutionBonus;
            quest.IsActive = model.IsActive;

            await _questRepository.UpdateAsync(quest);
            await _questRepository.SaveChangesAsync();

            TempData["SuccessMessage"] = "Quest updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id)
    {
        var quest = await _questRepository.GetByIdAsync(id);
        if (quest == null)
        {
            return NotFound();
        }

        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null || quest.PartyId != domainUser.PartyId)
        {
            return Forbid();
        }

        await _questRepository.DeleteAsync(id);
        await _questRepository.SaveChangesAsync();

        TempData["SuccessMessage"] = "Quest deleted successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Verifications()
    {
        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var completions = await _questCompletionRepository.GetAllByConditionAsync(
            qc => qc.Quest!.PartyId == domainUser.PartyId && qc.Status == CompletionStatus.PendingVerification);

        return View(completions);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveCompletion(Guid id)
    {
        var completion = await _questCompletionRepository.GetByIdAsync(id);
        if (completion == null)
        {
            return NotFound();
        }

        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null)
        {
            return Forbid();
        }

        completion.Status = CompletionStatus.Approved;
        completion.VerifiedAt = DateTime.UtcNow;
        completion.VerifiedByDMId = domainUser.Id;

        await _questCompletionRepository.UpdateAsync(completion);
        await _questCompletionRepository.SaveChangesAsync();

        TempData["SuccessMessage"] = "Quest completion approved!";
        return RedirectToAction(nameof(Verifications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectCompletion(Guid id)
    {
        var completion = await _questCompletionRepository.GetByIdAsync(id);
        if (completion == null)
        {
            return NotFound();
        }

        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null)
        {
            return Forbid();
        }

        completion.Status = CompletionStatus.Rejected;
        completion.VerifiedAt = DateTime.UtcNow;
        completion.VerifiedByDMId = domainUser.Id;

        await _questCompletionRepository.UpdateAsync(completion);
        await _questCompletionRepository.SaveChangesAsync();

        TempData["SuccessMessage"] = "Quest completion rejected!";
        return RedirectToAction(nameof(Verifications));
    }
}
