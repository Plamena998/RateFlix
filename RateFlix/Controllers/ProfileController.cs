using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RateFlix.Core.Models;
using RateFlix.Core.ViewModels;
using RateFlix.Services.Interfaces;

public class ProfileController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IProfileService _profileService;

    public ProfileController(UserManager<AppUser> userManager, IProfileService profileService)
    {
        _userManager = userManager;
        _profileService = profileService;
    }

    public async Task<IActionResult> Index()
    {
        if (!User.Identity.IsAuthenticated)
        {
            ViewData["Title"] = "Login Required";
            return View("LoginPromptPage"); 
        }
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return View("_LoginPrompt");

        var model = await _profileService.GetUserProfileAsync(user.Id);
        if (model == null) return View("_LoginPrompt");

        model.UserName = user.UserName ?? "User";
        model.Email = user.Email;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequest request)
    {
        if (!User.Identity.IsAuthenticated)
            return Json(new ToggleFavoriteResponse { IsFavorite = false, Message = "Please log in" });

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return Json(new ToggleFavoriteResponse { IsFavorite = false, Message = "User not authenticated" });

        var result = await _profileService.ToggleFavoriteAsync(user.Id, request.ContentId);
        return Json(result);
    }

    [HttpGet]
    public async Task<IActionResult> IsFavorite(int contentId)
    {
        if (!User.Identity.IsAuthenticated) return Json(new { isFavorite = false });

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Json(new { isFavorite = false });

        var isFav = await _profileService.IsFavoriteAsync(user.Id, contentId);
        return Json(new { isFavorite = isFav });
    }

    [HttpGet]
    public IActionResult LoginPrompt() => PartialView("_LoginPrompt");
}
