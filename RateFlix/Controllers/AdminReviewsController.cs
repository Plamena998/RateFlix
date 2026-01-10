using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateFlix.Data;
using RateFlix.Infrastructure;
using RateFlix.Models.ViewModels.Admin;

namespace RateFlix.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminReviewsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminReviewsController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AdminReviews
        public async Task<IActionResult> Index(string search = "", int? rating = null, int page = 1)
        {
            int pageSize = 20;
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Content)
                .Where(r => r.DeletedAt == null)
                .AsQueryable();

            // Search filter (by content title or user)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Content.Title.Contains(search) ||
                                        r.User.UserName.Contains(search));
            }

            // Rating filter
            if (rating.HasValue)
            {
                query = query.Where(r => r.Rating == rating.Value);
            }

            var totalReviews = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalReviews / (double)pageSize);

            var reviews = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new AdminReviewListViewModel
                {
                    Id = r.Id,
                    UserName = r.User.UserName,
                    UserId = r.UserId,
                    ContentTitle = r.Content.Title,
                    ContentId = r.ContentId,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    ContentType = r.Content is RateFlix.Data.Models.Movie ? "Movie" : "Series"
                })
                .ToListAsync();

            var model = new AdminReviewsIndexViewModel
            {
                Reviews = reviews,
                Search = search,
                SelectedRating = rating,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalReviews = totalReviews
            };

            return View(model);
        }

        // POST: AdminReviews/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null)
                return NotFound();

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Review deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST: AdminReviews/BanUser/userId
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUserFromComments(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            // Ban user from commenting (lockout for 100 years)
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

            TempData["Success"] = $"User {user.UserName} has been banned from commenting.";
            return RedirectToAction(nameof(Index));
        }
    }
}