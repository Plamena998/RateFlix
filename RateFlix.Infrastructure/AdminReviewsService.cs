using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RateFlix.Core.Models;
using RateFlix.Core.ViewModels.Admin;
using RateFlix.Data;
using RateFlix.Services.Interfaces;

namespace RateFlix.Services
{
    public class AdminReviewsService : IAdminReviewsService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminReviewsService(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminReviewsIndexViewModel> GetReviewsAsync(string search = "", int? rating = null, int page = 1, int pageSize = 20)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Content)
                .Where(r => r.DeletedAt == null)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(r => r.Content.Title.Contains(search) || r.User.UserName.Contains(search));
            }

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
                    ContentType = r.Content is Movie ? "Movie" : "Series"
                })
                .ToListAsync();

            return new AdminReviewsIndexViewModel
            {
                Reviews = reviews,
                Search = search,
                SelectedRating = rating,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalReviews = totalReviews
            };
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review == null) return false;

            _context.Reviews.Remove(review);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> BanUserFromCommentsAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return false;

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            return true;
        }
    }
}
