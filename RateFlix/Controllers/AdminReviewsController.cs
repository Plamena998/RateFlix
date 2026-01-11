using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateFlix.Services.Interfaces;

[Authorize(Roles = "Administrator")]
public class AdminReviewsController : Controller
{
    private readonly IAdminReviewsService _reviewsService;

    public AdminReviewsController(IAdminReviewsService reviewsService)
    {
        _reviewsService = reviewsService;
    }

    public async Task<IActionResult> Index(string search = "", int? rating = null, int page = 1)
    {
        var model = await _reviewsService.GetReviewsAsync(search, rating, page);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _reviewsService.DeleteReviewAsync(id);
        TempData["Success"] = success ? "Review deleted successfully!" : "Review not found.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> BanUserFromComments(string userId)
    {
        var success = await _reviewsService.BanUserFromCommentsAsync(userId);
        TempData["Success"] = success ? $"User has been banned from commenting." : "User not found.";
        return RedirectToAction(nameof(Index));
    }
}
