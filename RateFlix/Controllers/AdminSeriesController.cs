using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateFlix.Core.ViewModels.Admin;
using RateFlix.Services.Interfaces;

[Authorize(Roles = "Administrator")]
public class AdminSeriesController : Controller
{
    private readonly IAdminSeriesService _seriesService;

    public AdminSeriesController(IAdminSeriesService seriesService)
    {
        _seriesService = seriesService;
    }

    public async Task<IActionResult> Index(string search = "", int page = 1)
    {
        var model = await _seriesService.GetSeriesAsync(search, page);
        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var model = await _seriesService.GetSeriesFormAsync();
        return View("Create", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminSeriesFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model = await _seriesService.GetSeriesFormAsync();
            return View("Form", model);
        }

        await _seriesService.CreateSeriesAsync(model);
        TempData["Success"] = "Series created successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _seriesService.GetSeriesFormAsync(id);
        if (model == null) return NotFound();
        return View("Edit", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminSeriesFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model = await _seriesService.GetSeriesFormAsync(id);
            return View("Edit", model);
        }

        await _seriesService.UpdateSeriesAsync(model);
        TempData["Success"] = "Series updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _seriesService.DeleteSeriesAsync(id);
        TempData["Success"] = "Series deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
