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
    public class MoviesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        public MoviesController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(
            string? search,
            int? genreId,
            int? year,
            string? sortBy,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Movies
                .Include(m => m.ContentGenres)
                    .ThenInclude(cg => cg.Genre)
                .Include(m => m.Director)
                .AsQueryable();

            // Филтър по търсене
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Title.Contains(search));
            }

            // Филтър по жанр
            if (genreId.HasValue)
            {
                query = query.Where(m => m.ContentGenres.Any(cg => cg.GenreId == genreId.Value));
            }

            // Филтър по година
            if (year.HasValue)
            {
                query = query.Where(m => m.ReleaseYear == year.Value);
            }

            // Сортиране
            query = sortBy switch
            {
                "title" => query.OrderBy(m => m.Title),
                "year_desc" => query.OrderByDescending(m => m.ReleaseYear),
                "year_asc" => query.OrderBy(m => m.ReleaseYear),
                "rating_asc" => query.OrderBy(m => m.IMDBScore),
                _ => query.OrderByDescending(m => m.IMDBScore)
            };

            var totalMovies = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

            var movies = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();

            var model = new MoviesIndexViewModel
            {
                Movies = movies,
                Genres = genres,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalMovies = totalMovies,
                PageSize = pageSize,
                Search = search,
                SelectedGenreId = genreId,
                SelectedYear = year,
                SortBy = sortBy
            };

            return View(model);
        }

        // AJAX endpoint за пагинация и филтриране
        [HttpGet]
        public async Task<IActionResult> LoadPage(
            string? search,
            int? genreId,
            int? year,
            string? sortBy,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Movies.AsQueryable();

            // Филтри
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(m => m.Title.Contains(search));
            }

            if (genreId.HasValue)
            {
                query = query.Where(m => m.ContentGenres.Any(cg => cg.GenreId == genreId.Value));
            }

            if (year.HasValue)
            {
                query = query.Where(m => m.ReleaseYear == year.Value);
            }

            // Сортиране
            query = sortBy switch
            {
                "title" => query.OrderBy(m => m.Title),
                "year_desc" => query.OrderByDescending(m => m.ReleaseYear),
                "year_asc" => query.OrderBy(m => m.ReleaseYear),
                "rating_asc" => query.OrderBy(m => m.IMDBScore),
                _ => query.OrderByDescending(m => m.IMDBScore)
            };

            var totalMovies = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

            var movies = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new
                {
                    id = m.Id,
                    title = m.Title,
                    year = m.ReleaseYear,
                    rating = m.IMDBScore,
                    image = m.ImageUrl,
                    duration = m.Duration
                })
                .ToListAsync();

            return Json(new
            {
                movies = movies,
                currentPage = page,
                totalPages = totalPages,
                totalMovies = totalMovies
            });
        }

        // AJAX - Get Content Modal Data (Universal)
        [HttpGet]
        public async Task<IActionResult> GetContentModal(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.Director)
                .Include(m => m.ContentGenres)
                    .ThenInclude(cg => cg.Genre)
                .Include(m => m.ContentActors)
                    .ThenInclude(ca => ca.Actor)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound();
            }

            return PartialView("Components/_ContentModal", movie);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> SubmitRating(int movieId, int score, string review)
        {
            // Get current user
            var userId = _userManager.GetUserId(User);

            // Get the movie
            var movie = await _context.Movies
                .Include(m => m.Reviews) // make sure Ratings navigation exists
                .FirstOrDefaultAsync(m => m.Id == movieId);

            if (movie == null) return Json(new { success = false, message = "Movie not found" });

            //Add or update user's rating
            var existingRating = movie.Reviews.FirstOrDefault(r => r.UserId == userId);
            if (existingRating != null)
            {
                existingRating.Rating = score;
                existingRating.Comment = review ?? "";
                existingRating.CreatedAt = DateTime.UtcNow;
            }
            else
            {
                movie.Reviews.Add(new Review
                {
                    ContentId = movieId,
                    UserId = userId,
                    Rating = score,
                    Comment = review ?? "",
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();

            // Calculate weighted average including API score
            // Assign a weight for API score (adjustable)
            const int apiWeight = 100;
            var apiScore = movie.IMDBScore; // original API score

            var sumUserScores = movie.Reviews.Sum(r => r.Rating);
            var numUserRatings = movie.Reviews.Count;

            var newAverage = ((apiScore * apiWeight) + sumUserScores) / (apiWeight + numUserRatings);

            movie.IMDBScore = Math.Round(newAverage, 1);
            await _context.SaveChangesAsync();

            return Json(new
            {
                success = true,
                newRating = movie.IMDBScore,
                numUserRatings = movie.Reviews.Count
            });
        }

    }
}