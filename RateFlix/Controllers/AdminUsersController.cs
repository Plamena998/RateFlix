using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RateFlix.Core.Models;
using RateFlix.Services.Interfaces;

[Authorize(Roles = "Administrator")]
public class AdminUsersController : Controller
{
    private readonly IAdminUsersService _usersService;
    private readonly UserManager<AppUser> _userManager;

    public AdminUsersController(IAdminUsersService usersService, UserManager<AppUser> userManager)
    {
        _usersService = usersService;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string search = "", string status = "", int page = 1)
    {
        var model = await _usersService.GetUsersAsync(search, status, page);
        return View(model);
    }

    public async Task<IActionResult> Details(string id)
    {
        var model = await _usersService.GetUserDetailsAsync(id);
        if (model == null) return NotFound();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BanUser(string id)
    {
        await _usersService.BanUserAsync(id);
        TempData["Success"] = "User banned successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UnbanUser(string id)
    {
        await _usersService.UnbanUserAsync(id);
        TempData["Success"] = "User unbanned successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        await _usersService.DeleteUserAsync(id, currentUser.Id);
        TempData["Success"] = "User deleted successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Admins()
    {
        var admins = await _usersService.GetAdminUsersAsync();
        return View(admins);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeAdmin(string id)
    {
        await _usersService.MakeAdminAsync(id);
        TempData["Success"] = "User promoted to admin.";
        return RedirectToAction(nameof(Admins));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAdmin(string id)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        await _usersService.RemoveAdminAsync(id, currentUser.Id);
        TempData["Success"] = "Admin rights removed.";
        return RedirectToAction(nameof(Admins));
    }
}
