using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RateFlix.Data;
using RateFlix.Infrastructure;

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
    public async Task<IActionResult> Create(int contentId, int rating, string reviewText)
    {
        var userId = _userManager.GetUserId(User);
        if (userId == null)
            return Unauthorized();

        var review = new Review
        {
            ContentId = contentId,
            UserId = userId,
            Rating = rating,
            Comment = reviewText ?? "",
            CreatedAt = DateTime.UtcNow
        };

        _context.Reviews.Add(review);

        // Update the content rating
        var content = await _context.Reviews.FindAsync(contentId);
        if (content != null)
        {
            var allRatings = _context.Reviews.Where(r => r.ContentId == contentId).Select(r => r.Rating).ToList();
            content.Rating = (allRatings.Sum() + content.Rating) / (allRatings.Count + 1);
        }

        await _context.SaveChangesAsync();

        return Json(new { success = true, newRating = content.Rating });
    }
}
