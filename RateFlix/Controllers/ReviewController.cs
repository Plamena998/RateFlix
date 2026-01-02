using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateFlix.Data;
using RateFlix.Data.Models;
using RateFlix.Infrastructure;
using System.Linq;

[Route("[controller]/[action]")]
public class ReviewsController : Controller
{
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public ReviewsController(AppDbContext context, UserManager<AppUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public IActionResult SubmitReview(int contentId, int score = 0, string review = "")
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Json(new { success = false, message = "You must be logged in to submit a review." });

        // Check that at least a rating or a review text is provided
        if (score <= 0 && string.IsNullOrWhiteSpace(review))
        {
            return Json(new { success = false, message = "Please provide a rating or a comment." });
        }

        try
        {
            // Check if the user already reviewed this content
            var existingReview = _context.Reviews
                                         .FirstOrDefault(r => r.ContentId == contentId && r.UserId == userId);

            if (existingReview != null)
            {
                // Update existing review
                if (score > 0) existingReview.Rating = score;
                if (!string.IsNullOrWhiteSpace(review)) existingReview.Comment = review;
                existingReview.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new review
                var newReview = new Review
                {
                    ContentId = contentId,
                    UserId = userId,
                    Rating = score,
                    Comment = review ?? string.Empty,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Reviews.Add(newReview);
            }

            _context.SaveChanges();

            // Recalculate average rating
            var avgRating = _context.Reviews
                                    .Where(r => r.ContentId == contentId)
                                    .Average(r => r.Rating);

            return Json(new { success = true, newRating = avgRating });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = "Failed to save review: " + ex.Message });
        }
    }
}
