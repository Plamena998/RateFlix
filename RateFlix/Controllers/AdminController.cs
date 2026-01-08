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
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Dashboard
        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardViewModel
            {
                // User Statistics
                TotalUsers = await _context.Users.CountAsync(),
                NewUsersThisMonth = await _context.Users
                    .Where(u => u.CreatedAt.Month == DateTime.UtcNow.Month &&
                               u.CreatedAt.Year == DateTime.UtcNow.Year)
                    .CountAsync(),
                ActiveUsers = await _context.Users
                    .Where(u => u.UpdatedAt >= DateTime.UtcNow.AddDays(-30))
                    .CountAsync(),

                // Content Statistics
                TotalMovies = await _context.Movies.CountAsync(),
                TotalSeries = await _context.Series.CountAsync(),
                TotalActors = await _context.Actors.CountAsync(),

                // Review Statistics
                TotalReviews = await _context.Reviews.CountAsync(),
                ReviewsThisMonth = await _context.Reviews
                    .Where(r => r.CreatedAt.Month == DateTime.UtcNow.Month &&
                               r.CreatedAt.Year == DateTime.UtcNow.Year)
                    .CountAsync(),

                // Recent Activity
                RecentReviews = await _context.Reviews
                    .Include(r => r.User)
                    .Include(r => r.Content)
                    .OrderByDescending(r => r.CreatedAt)
                    .Take(5)
                    .Select(r => new RecentReviewViewModel
                    {
                        Id = r.Id,
                        UserName = r.User.UserName,
                        ContentTitle = r.Content.Title,
                        Rating = r.Rating,
                        Comment = r.Comment,
                        CreatedAt = r.CreatedAt
                    })
                    .ToListAsync(),

                RecentUsers = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Take(5)
                    .Select(u => new RecentUserViewModel
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync()
            };

            return View(model);
        }
    }
}