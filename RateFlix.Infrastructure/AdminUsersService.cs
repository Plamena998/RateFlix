using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RateFlix.Core.Models;
using RateFlix.Core.ViewModels;
using RateFlix.Core.ViewModels.Admin;
using RateFlix.Data;
using RateFlix.Services.Interfaces;

namespace RateFlix.Services
{
    public class AdminUsersService : IAdminUsersService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminUsersService(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<AdminUsersIndexViewModel> GetUsersAsync(string search = "", string status = "", int page = 1, int pageSize = 20)
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));

            if (!string.IsNullOrEmpty(status))
            {
                if (status == "banned")
                    query = query.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);
                else if (status == "active")
                    query = query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);
            }

            var totalUsers = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

            var users = await query
                .OrderByDescending(u => u.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var userViewModels = new List<AdminUserListViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var reviewCount = await _context.Reviews.CountAsync(r => r.UserId == user.Id);
                var favoriteCount = await _context.FavoriteContentя.CountAsync(f => f.UserId == user.Id);

                userViewModels.Add(new AdminUserListViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "Unknown",
                    Email = user.Email ?? "No Email",
                    CreatedAt = user.CreatedAt,
                    UpdatedAt = user.UpdatedAt,
                    IsBanned = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                    IsAdmin = roles.Contains("Administrator"),
                    TotalReviews = reviewCount,
                    TotalFavorites = favoriteCount
                });
            }

            return new AdminUsersIndexViewModel
            {
                Users = userViewModels,
                Search = search,
                Status = status,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalUsers = totalUsers
            };
        }

        public async Task<AdminUserDetailsViewModel?> GetUserDetailsAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return null;

            var roles = await _userManager.GetRolesAsync(user);

            var reviews = await _context.Reviews
                .Where(r => r.UserId == id)
                .Include(r => r.Content)
                .OrderByDescending(r => r.CreatedAt)
                .Take(10)
                .Select(r => new UserReviewViewModel
                {
                    ContentId = r.Content.Id,
                    Title = r.Content.Title,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IMDBScore = r.Content.IMDBScore,
                    ContentType = r.Content is Movie ? "Movies" : "Series"
                })
                .ToListAsync();

            var favorites = await _context.FavoriteContentя
                .Where(f => f.UserId == id)
                .Include(f => f.Content)
                .OrderByDescending(f => f.DateAdded)
                .Take(10)
                .Select(f => new FavoriteContentViewModel
                {
                    ContentId = f.Content.Id,
                    Title = f.Content.Title,
                    ImageUrl = f.Content.ImageUrl,
                    IMDBScore = f.Content.IMDBScore,
                    ReleaseYear = f.Content.ReleaseYear,
                    DateAdded = f.DateAdded,
                    ContentType = f.Content is Movie ? "Movies" : "Series",
                    Genres = new List<string>()
                })
                .ToListAsync();

            return new AdminUserDetailsViewModel
            {
                Id = user.Id,
                UserName = user.UserName ?? "Unknown",
                Email = user.Email ?? "No Email",
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                IsBanned = user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow,
                IsAdmin = roles.Contains("Administrator"),
                Reviews = reviews,
                Favorites = favorites,
                TotalReviews = await _context.Reviews.CountAsync(r => r.UserId == id),
                TotalFavorites = await _context.FavoriteContentя.CountAsync(f => f.UserId == id)
            };
        }

        public async Task<bool> BanUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
            return true;
        }

        public async Task<bool> UnbanUserAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            await _userManager.SetLockoutEndDateAsync(user, null);
            return true;
        }

        public async Task<bool> DeleteUserAsync(string id, string currentUserId)
        {
            if (id == currentUserId) return false;

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            await _userManager.DeleteAsync(user);
            return true;
        }

        public async Task<List<AdminUserListViewModel>> GetAdminUsersAsync()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var admins = new List<AdminUserListViewModel>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Administrator"))
                {
                    admins.Add(new AdminUserListViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? "Unknown",
                        Email = user.Email ?? "No Email",
                        CreatedAt = user.CreatedAt,
                        IsAdmin = true
                    });
                }
            }

            return admins;
        }

        public async Task<bool> MakeAdminAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            if (!await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                await _userManager.AddToRoleAsync(user, "Administrator");
            }

            return true;
        }

        public async Task<bool> RemoveAdminAsync(string id, string currentUserId)
        {
            if (id == currentUserId) return false;

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            if (await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Administrator");
            }

            return true;
        }
    }
}
