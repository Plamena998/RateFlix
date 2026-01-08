using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateFlix.Data;
using RateFlix.Data.Models;
using RateFlix.Infrastructure;
using RateFlix.Models.ViewModels;

namespace RateFlix.Controllers
{
    public class ProfileController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ProfileController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is authenticated
            if (!User.Identity.IsAuthenticated)
            {
                return View("LoginPrompt");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return View("LoginPrompt");
            }

            // Get user reviews
            var reviews = await _context.Reviews
                .Where(r => r.UserId == user.Id && r.DeletedAt == null)
                .Include(r => r.Content)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new UserReviewViewModel
                {
                    ContentId = r.Content.Id,
                    Title = r.Content.Title,
                    ImageUrl = r.Content.ImageUrl,
                    Rating = r.Rating,
                    Comment = r.Comment,
                    CreatedAt = r.CreatedAt,
                    IMDBScore = r.Content.IMDBScore,
                    ContentType = r.Content is Movie ? "Movies" : "Series"
                })
                .ToListAsync();

            // Get user favorites
            var favorites = await _context.FavoriteContentя
                .Where(f => f.UserId == user.Id)
                .Include(f => f.Content)
                    .ThenInclude(c => c.ContentGenres)
                        .ThenInclude(cg => cg.Genre)
                .OrderByDescending(f => f.DateAdded)
                .Select(f => new FavoriteContentViewModel
                {
                    ContentId = f.Content.Id,
                    Title = f.Content.Title,
                    ImageUrl = f.Content.ImageUrl,
                    IMDBScore = f.Content.IMDBScore,
                    ReleaseYear = f.Content.ReleaseYear,
                    DateAdded = f.DateAdded,
                    ContentType = f.Content is Movie ? "Movies" : "Series",
                    Genres = f.Content.ContentGenres.Select(cg => cg.Genre.Name).ToList()
                })
                .ToListAsync();

            // Calculate statistics
            var genreStats = favorites
                .SelectMany(f => f.Genres)
                .GroupBy(g => g)
                .Select(g => new GenreStatViewModel
                {
                    Genre = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .Take(5)
                .ToList();

            var totalGenres = genreStats.Sum(g => g.Count);
            foreach (var stat in genreStats)
            {
                stat.Percentage = totalGenres > 0 ? (stat.Count * 100.0 / totalGenres) : 0;
            }

            var statistics = new ProfileStatisticsViewModel
            {
                TotalReviews = reviews.Count,
                TotalFavorites = favorites.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                TopGenres = genreStats,
                TotalMovies = favorites.Count(f => f.ContentType == "Movies"),
                TotalSeries = favorites.Count(f => f.ContentType == "Series")
            };

            var model = new UserProfileViewModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? "User",
                Email = user.Email,
                Reviews = reviews,
                Favorites = favorites,
                Statistics = statistics
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFavorite([FromBody] ToggleFavoriteRequest request)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new ToggleFavoriteResponse { IsFavorite = false, Message = "Please log in to add favorites" });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Json(new ToggleFavoriteResponse { IsFavorite = false, Message = "User not authenticated" });

            var existingFavorite = await _context.FavoriteContentя
                .FirstOrDefaultAsync(f => f.UserId == user.Id && f.ContentId == request.ContentId);

            if (existingFavorite != null)
            {
                _context.FavoriteContentя.Remove(existingFavorite);
                await _context.SaveChangesAsync();
                return Json(new ToggleFavoriteResponse
                {
                    IsFavorite = false,
                    Message = "Removed from favorites"
                });
            }
            else
            {
                var favorite = new FavoriteContent
                {
                    UserId = user.Id,
                    ContentId = request.ContentId,
                    DateAdded = DateTime.UtcNow
                };
                _context.FavoriteContentя.Add(favorite);
                await _context.SaveChangesAsync();
                return Json(new ToggleFavoriteResponse
                {
                    IsFavorite = true,
                    Message = "Added to favorites"
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> IsFavorite(int contentId)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { isFavorite = false });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Json(new { isFavorite = false });

            var isFavorite = await _context.FavoriteContentя
                .AnyAsync(f => f.UserId == user.Id && f.ContentId == contentId);

            return Json(new { isFavorite });
        }

        // Return login prompt for AJAX
        [HttpGet]
        public IActionResult LoginPrompt()
        {
            return PartialView("LoginPrompt");
        }
    }
}