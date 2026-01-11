using Microsoft.EntityFrameworkCore;
using RateFlix.Core.Models;
using RateFlix.Core.ViewModels;
using RateFlix.Data;
using RateFlix.Services.Interfaces;

namespace RateFlix.Services
{
    public class ProfileService : IProfileService
    {
        private readonly AppDbContext _context;

        public ProfileService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserProfileViewModel?> GetUserProfileAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId)) return null;

            // Reviews
            var reviews = await _context.Reviews
                .Where(r => r.UserId == userId && r.DeletedAt == null)
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

            // Favorites
            var favorites = await _context.FavoriteContentя
                .Where(f => f.UserId == userId)
                .Include(f => f.Content).ThenInclude(c => c.ContentGenres).ThenInclude(cg => cg.Genre)
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

            // Statistics
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
                stat.Percentage = totalGenres > 0 ? (stat.Count * 100.0 / totalGenres) : 0;

            var statistics = new ProfileStatisticsViewModel
            {
                TotalReviews = reviews.Count,
                TotalFavorites = favorites.Count,
                AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
                TopGenres = genreStats,
                TotalMovies = favorites.Count(f => f.ContentType == "Movies"),
                TotalSeries = favorites.Count(f => f.ContentType == "Series")
            };

            return new UserProfileViewModel
            {
                UserId = userId,
                UserName = "User", // TODO: get from Identity if needed
                Reviews = reviews,
                Favorites = favorites,
                Statistics = statistics
            };
        }

        public async Task<ToggleFavoriteResponse> ToggleFavoriteAsync(string userId, int contentId)
        {
            var existing = await _context.FavoriteContentя
                .FirstOrDefaultAsync(f => f.UserId == userId && f.ContentId == contentId);

            if (existing != null)
            {
                _context.FavoriteContentя.Remove(existing);
                await _context.SaveChangesAsync();
                return new ToggleFavoriteResponse { IsFavorite = false, Message = "Removed from favorites" };
            }

            var favorite = new FavoriteContent
            {
                UserId = userId,
                ContentId = contentId,
                DateAdded = DateTime.UtcNow
            };
            _context.FavoriteContentя.Add(favorite);
            await _context.SaveChangesAsync();
            return new ToggleFavoriteResponse { IsFavorite = true, Message = "Added to favorites" };
        }

        public async Task<bool> IsFavoriteAsync(string userId, int contentId)
        {
            return await _context.FavoriteContentя.AnyAsync(f => f.UserId == userId && f.ContentId == contentId);
        }
    }
}
