using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateFlix.Services.Interfaces;

[Authorize(Roles = "Administrator")]
public class AdminController : Controller
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    public async Task<IActionResult> Index()
    {
        var model = await _adminService.GetDashboardDataAsync();
        return View(model);
    }
}
