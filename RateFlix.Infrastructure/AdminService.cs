using Microsoft.EntityFrameworkCore;
using RateFlix.Core.ViewModels.Admin;
using RateFlix.Data;
using RateFlix.Services.Interfaces;

namespace RateFlix.Services
{
    public class AdminService : IAdminService
    {
        private readonly AppDbContext _context;

        public AdminService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminDashboardViewModel> GetDashboardDataAsync()
        {
            var now = DateTime.UtcNow;

            var recentReviewsQuery = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Content)
                .OrderByDescending(r => r.CreatedAt)
                .Take(5);

            var recentUsersQuery = _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .Take(5);

            var model = new AdminDashboardViewModel
            {
                TotalUsers = await _context.Users.CountAsync(),
                NewUsersThisMonth = await _context.Users
                    .Where(u => u.CreatedAt.Month == now.Month && u.CreatedAt.Year == now.Year)
                    .CountAsync(),
                ActiveUsers = await _context.Users
                    .Where(u => u.CreatedAt >= now.AddDays(-30))
                    .CountAsync(),

                TotalMovies = await _context.Movies.CountAsync(),
                TotalSeries = await _context.Series.CountAsync(),
                TotalActors = await _context.Actors.CountAsync(),

                TotalReviews = await _context.Reviews.CountAsync(),
                ReviewsThisMonth = await _context.Reviews
                    .Where(r => r.CreatedAt.Month == now.Month && r.CreatedAt.Year == now.Year)
                    .CountAsync(),

                RecentReviews = await recentReviewsQuery
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

                RecentUsers = await recentUsersQuery
                    .Select(u => new RecentUserViewModel
                    {
                        Id = u.Id,
                        UserName = u.UserName,
                        Email = u.Email,
                        CreatedAt = u.CreatedAt
                    })
                    .ToListAsync()
            };

            return model;
        }
    }
}
