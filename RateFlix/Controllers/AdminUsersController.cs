using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateFlix.Data;
using RateFlix.Infrastructure;
using RateFlix.Models.ViewModels;
using RateFlix.Models.ViewModels.Admin;

namespace RateFlix.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminUsersController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminUsersController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: AdminUsers
        public async Task<IActionResult> Index(string search = "", string status = "", int page = 1)
        {
            int pageSize = 20;
            var query = _context.Users.AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.UserName.Contains(search) || u.Email.Contains(search));
            }

            // Status filter
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "banned")
                {
                    query = query.Where(u => u.LockoutEnd != null && u.LockoutEnd > DateTimeOffset.UtcNow);
                }
                else if (status == "active")
                {
                    query = query.Where(u => u.LockoutEnd == null || u.LockoutEnd <= DateTimeOffset.UtcNow);
                }
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

            var model = new AdminUsersIndexViewModel
            {
                Users = userViewModels,
                Search = search,
                Status = status,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalUsers = totalUsers
            };

            return View(model);
        }

        // GET: AdminUsers/Details/id
        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

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
                    ContentType = r.Content is RateFlix.Data.Models.Movie ? "Movies" : "Series"
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
                    ContentType = f.Content is RateFlix.Data.Models.Movie ? "Movies" : "Series",
                    Genres = new List<string>()
                })
                .ToListAsync();

            var model = new AdminUserDetailsViewModel
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

            return View(model);
        }

        // POST: AdminUsers/BanUser/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Ban user for 100 years (permanent)
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

            TempData["Success"] = $"User {user.UserName} has been banned.";
            return RedirectToAction(nameof(Index));
        }

        // POST: AdminUsers/UnbanUser/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnbanUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["Success"] = $"User {user.UserName} has been unbanned.";
            return RedirectToAction(nameof(Index));
        }

        // POST: AdminUsers/Delete/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Don't allow deleting yourself
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == id)
            {
                TempData["Error"] = "You cannot delete your own account!";
                return RedirectToAction(nameof(Index));
            }

            await _userManager.DeleteAsync(user);

            TempData["Success"] = $"User {user.UserName} has been deleted.";
            return RedirectToAction(nameof(Index));
        }

        // GET: AdminUsers/Admins
        public async Task<IActionResult> Admins()
        {
            var allUsers = await _userManager.Users.ToListAsync();
            var adminUsers = new List<AdminUserListViewModel>();

            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Administrator"))
                {
                    adminUsers.Add(new AdminUserListViewModel
                    {
                        Id = user.Id,
                        UserName = user.UserName ?? "Unknown",
                        Email = user.Email ?? "No Email",
                        CreatedAt = user.CreatedAt,
                        IsAdmin = true
                    });
                }
            }

            return View(adminUsers);
        }

        // POST: AdminUsers/MakeAdmin/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                await _userManager.AddToRoleAsync(user, "Administrator");
                TempData["Success"] = $"{user.UserName} is now an administrator.";
            }

            return RedirectToAction(nameof(Admins));
        }

        // POST: AdminUsers/RemoveAdmin/id
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveAdmin(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            // Don't allow removing yourself
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser.Id == id)
            {
                TempData["Error"] = "You cannot remove your own admin privileges!";
                return RedirectToAction(nameof(Admins));
            }

            if (await _userManager.IsInRoleAsync(user, "Administrator"))
            {
                await _userManager.RemoveFromRoleAsync(user, "Administrator");
                TempData["Success"] = $"{user.UserName} is no longer an administrator.";
            }

            return RedirectToAction(nameof(Admins));
        }
    }
}