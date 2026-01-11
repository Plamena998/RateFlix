using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RateFlix.Core.Models;
using RateFlix.Services.Interfaces;

[Route("[controller]/[action]")]
public class ReviewsController : Controller
{
    private readonly UserManager<AppUser> _userManager;
    private readonly IReviewService _reviewService;

    public ReviewsController(UserManager<AppUser> userManager, IReviewService reviewService)
    {
        _userManager = userManager;
        _reviewService = reviewService;
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitReview(int contentId, int score = 0, string review = "")
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Json(new { success = false, message = "You must be logged in to submit a review." });

        var result = await _reviewService.SubmitReviewAsync(userId, contentId, score, review);

        return Json(new
        {
            success = result.Success,
            message = result.Message,
            newRating = result.NewRating
        });
    }
}
