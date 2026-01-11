using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateFlix.Core.ViewModels.Admin;
using RateFlix.Services.Interfaces;

[Authorize(Roles = "Administrator")]
public class AdminMoviesController : Controller
{
    private readonly IAdminMovieService _movieService;

    public AdminMoviesController(IAdminMovieService movieService)
    {
        _movieService = movieService;
    }

    public async Task<IActionResult> Index(string search = "", int page = 1)
    {
        var model = await _movieService.GetMoviesAsync(search, page);
        return View(model);
    }

    public async Task<IActionResult> Create()
    {
        var model = await _movieService.GetMovieFormAsync();
        return View("Create", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AdminMovieFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model = await _movieService.GetMovieFormAsync();
            return View("Create", model);
        }

        await _movieService.CreateMovieAsync(model);
        TempData["Success"] = "Movie created successfully!";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var model = await _movieService.GetMovieFormAsync(id);
        if (model == null) return NotFound();
        return View("Edit", model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, AdminMovieFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model = await _movieService.GetMovieFormAsync(id);
            return View("Edit", model);
        }

        await _movieService.UpdateMovieAsync(model);
        TempData["Success"] = "Movie updated successfully!";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        await _movieService.DeleteMovieAsync(id);
        TempData["Success"] = "Movie deleted successfully!";
        return RedirectToAction(nameof(Index));
    }
}
