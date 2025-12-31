using ChoreWars.Core.Entities;
using ChoreWars.Core.Interfaces;
using ChoreWars.Infrastructure.Identity;
using ChoreWars.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChoreWars.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IUserRepository _userRepository;
    private readonly IRepository<Party> _partyRepository;

    public AccountController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IUserRepository userRepository,
        IRepository<Party> partyRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _userRepository = userRepository;
        _partyRepository = partyRepository;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                model.Username,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                return RedirectToLocal(returnUrl);
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Validate that if IsDungeonMaster is true, PartyName is provided
            if (model.IsDungeonMaster && string.IsNullOrWhiteSpace(model.PartyName))
            {
                ModelState.AddModelError(nameof(model.PartyName), "Party name is required when registering as Dungeon Master.");
                return View(model);
            }

            // Validate that if not DM, either invite code is provided or they want to create a party
            if (!model.IsDungeonMaster && string.IsNullOrWhiteSpace(model.InviteCode) && string.IsNullOrWhiteSpace(model.PartyName))
            {
                ModelState.AddModelError(nameof(model.InviteCode), "Please provide a party invite code or create a new party.");
                return View(model);
            }

            Party? party = null;

            // If invite code provided, find the party
            if (!string.IsNullOrWhiteSpace(model.InviteCode))
            {
                party = await _partyRepository.GetByConditionAsync(p => p.InviteCode == model.InviteCode);
                if (party == null)
                {
                    ModelState.AddModelError(nameof(model.InviteCode), "Invalid invite code.");
                    return View(model);
                }
            }
            // If DM or wants to create new party, create one
            else if (!string.IsNullOrWhiteSpace(model.PartyName))
            {
                party = new Party
                {
                    Id = Guid.NewGuid(),
                    Name = model.PartyName,
                    InviteCode = GenerateInviteCode(),
                    DungeonMasterId = Guid.Empty // Will be set after user creation
                };
            }

            if (party == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to create or join party.");
                return View(model);
            }

            // Create domain user
            var domainUser = new User
            {
                Id = Guid.NewGuid(),
                Username = model.Username,
                DisplayName = model.DisplayName,
                PartyId = party.Id,
                CurrentLevel = 1,
                CurrentXP = 0,
                XPToNextLevel = 100,
                TotalGold = 0
            };

            // If creating new party, set the DM
            if (party.DungeonMasterId == Guid.Empty)
            {
                party.DungeonMasterId = domainUser.Id;
                await _partyRepository.AddAsync(party);
                await _partyRepository.SaveChangesAsync();
            }

            // Add domain user
            await _userRepository.AddAsync(domainUser);
            await _userRepository.SaveChangesAsync();

            // Create Identity user
            var identityUser = new ApplicationUser
            {
                UserName = model.Username,
                DomainUserId = domainUser.Id
            };

            var result = await _userManager.CreateAsync(identityUser, model.Password);

            if (result.Succeeded)
            {
                // Add DM role if applicable
                if (model.IsDungeonMaster || party.DungeonMasterId == domainUser.Id)
                {
                    await _userManager.AddToRoleAsync(identityUser, "DungeonMaster");
                }

                await _signInManager.SignInAsync(identityUser, isPersistent: false);
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return RedirectToAction("Index", "Home");
        }
    }

    private string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Repeat(chars, 8)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
