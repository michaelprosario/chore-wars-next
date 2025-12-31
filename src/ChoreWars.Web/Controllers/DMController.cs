using ChoreWars.Core.Commands;
using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Infrastructure.Identity;
using ChoreWars.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ChoreWars.Web.Controllers;

[Authorize(Roles = "DungeonMaster")]
public class DMController : Controller
{
    private readonly IQuestRepository _questRepository;
    private readonly IUserRepository _userRepository;
    private readonly IRepository<QuestCompletion> _questCompletionRepository;
    private readonly IRepository<Party> _partyRepository;
    private readonly IQuestService _questService;
    private readonly UserManager<ApplicationUser> _userManager;

    public DMController(
        IQuestRepository questRepository,
        IUserRepository userRepository,
        IRepository<QuestCompletion> questCompletionRepository,
        IRepository<Party> partyRepository,
        IQuestService questService,
        UserManager<ApplicationUser> userManager)
    {
        _questRepository = questRepository;
        _userRepository = userRepository;
        _questCompletionRepository = questCompletionRepository;
        _partyRepository = partyRepository;
        _questService = questService;
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
        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null)
        {
            return Forbid();
        }

        var result = await _questService.VerifyQuestAsync(new VerifyQuestCommand
        {
            QuestCompletionId = id,
            DMId = domainUser.Id,
            IsApproved = true
        });

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = result.Message ?? "Quest completion approved!";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", result.Errors);
        }

        return RedirectToAction(nameof(Verifications));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectCompletion(Guid id)
    {
        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null)
        {
            return Forbid();
        }

        var result = await _questService.VerifyQuestAsync(new VerifyQuestCommand
        {
            QuestCompletionId = id,
            DMId = domainUser.Id,
            IsApproved = false
        });

        if (result.IsSuccess)
        {
            TempData["SuccessMessage"] = result.Message ?? "Quest completion rejected!";
        }
        else
        {
            TempData["ErrorMessage"] = string.Join(", ", result.Errors);
        }

        return RedirectToAction(nameof(Verifications));
    }

    [HttpGet]
    public IActionResult Import()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Import(IFormFile? jsonFile)
    {
        if (jsonFile == null || jsonFile.Length == 0)
        {
            ModelState.AddModelError("", "Please select a JSON file to import");
            return View();
        }

        // Validate file extension
        if (!jsonFile.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            ModelState.AddModelError("", "Only .json files are accepted");
            return View();
        }

        // Validate file size (5MB limit)
        if (jsonFile.Length > 5 * 1024 * 1024)
        {
            ModelState.AddModelError("", "File size must be less than 5MB");
            return View();
        }

        var domainUser = await GetCurrentDomainUserAsync();
        if (domainUser == null)
        {
            return RedirectToAction("Login", "Account");
        }

        try
        {
            // Read file content
            string jsonContent;
            using (var reader = new StreamReader(jsonFile.OpenReadStream()))
            {
                jsonContent = await reader.ReadToEndAsync();
            }

            // Parse JSON
            JsonDocument jsonDoc;
            try
            {
                jsonDoc = JsonDocument.Parse(jsonContent);
            }
            catch (JsonException)
            {
                ModelState.AddModelError("", "Invalid JSON format. Please check your file syntax.");
                return View();
            }

            // Extract quests array
            if (!jsonDoc.RootElement.TryGetProperty("quests", out var questsElement))
            {
                ModelState.AddModelError("", "JSON must contain a 'quests' array");
                return View();
            }

            if (questsElement.ValueKind != JsonValueKind.Array)
            {
                ModelState.AddModelError("", "'quests' must be an array");
                return View();
            }

            var questsList = new List<QuestImportDto>();
            foreach (var questElement in questsElement.EnumerateArray())
            {
                try
                {
                    var questImport = new QuestImportDto
                    {
                        Title = questElement.TryGetProperty("title", out var titleProp) ? titleProp.GetString() ?? "" : "",
                        Description = questElement.TryGetProperty("description", out var descProp) ? descProp.GetString() ?? "" : "",
                        XPReward = questElement.TryGetProperty("xpReward", out var xpProp) ? xpProp.GetInt32() : 0,
                        GoldReward = questElement.TryGetProperty("goldReward", out var goldProp) ? goldProp.GetInt32() : 0,
                        Difficulty = questElement.TryGetProperty("difficulty", out var diffProp) ? diffProp.GetString() ?? "" : "",
                        QuestType = questElement.TryGetProperty("questType", out var typeProp) ? typeProp.GetString() : null,
                        StrengthBonus = questElement.TryGetProperty("strengthBonus", out var strProp) ? strProp.GetInt32() : 0,
                        IntelligenceBonus = questElement.TryGetProperty("intelligenceBonus", out var intProp) ? intProp.GetInt32() : 0,
                        ConstitutionBonus = questElement.TryGetProperty("constitutionBonus", out var conProp) ? conProp.GetInt32() : 0
                    };
                    questsList.Add(questImport);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error parsing quest data: {ex.Message}");
                    return View();
                }
            }

            if (questsList.Count == 0)
            {
                ModelState.AddModelError("", "No quests found in file");
                return View();
            }

            // Limit to 100 quests to prevent abuse
            if (questsList.Count > 100)
            {
                ModelState.AddModelError("", "Maximum of 100 quests can be imported at once");
                return View();
            }

            // Call service to import quests
            var command = new ImportQuestsCommand
            {
                Quests = questsList,
                PartyId = domainUser.PartyId,
                DMId = domainUser.Id
            };

            var result = await _questService.ImportQuestsAsync(command);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = result.Message ?? $"Successfully imported {result.Data?.Count ?? 0} quest(s)";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                // Handle validation errors
                if (result.ValidationErrors.Any())
                {
                    foreach (var validationError in result.ValidationErrors)
                    {
                        ModelState.AddModelError("", $"{validationError.Field}: {validationError.Message}");
                    }
                }
                else if (result.Errors.Any())
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                return View();
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"An error occurred while importing quests: {ex.Message}");
            return View();
        }
    }
}
