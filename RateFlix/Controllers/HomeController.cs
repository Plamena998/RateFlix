using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateFlix.Data;
using RateFlix.Data.Models;
using RateFlix.Models.ViewModels;

namespace RateFlix.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var model = new HomeViewModel
            {

                TopMovies = await _context.Movies
                    .OrderByDescending(m => m.IMDBScore)
                    .Take(10)
                    .ToListAsync(),

                TopSeries = await _context.Series
                    .OrderByDescending(s => s.IMDBScore)
                    .Take(10)
                    .ToListAsync(),

                TrendingMovies = await _context.Movies
                    .OrderByDescending(m => m.ReleaseYear)
                    .ThenByDescending(m => m.IMDBScore)
                    .Take(10)
                    .ToListAsync(),

                NewReleases = await _context.Series
                    .OrderByDescending(s => s.ReleaseYear)
                    .Take(10)
                    .ToListAsync()
            };

            return View(model);
        }

        // AJAX endpoint за търсене
        [HttpGet]
        public async Task<IActionResult> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new { movies = new List<object>(), series = new List<object>() });

            var movies = await _context.Movies
                .Where(m => m.Title.Contains(query))
                .Take(5)
                .Select(m => new
                {
                    id = m.Id,
                    title = m.Title,
                    year = m.ReleaseYear,
                    rating = m.IMDBScore,
                    image = m.ImageUrl,
                    type = "movie"
                })
                .ToListAsync();

            var series = await _context.Series
                .Where(s => s.Title.Contains(query))
                .Take(5)
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    year = s.ReleaseYear,
                    rating = s.IMDBScore,
                    image = s.ImageUrl,
                    type = "series"
                })
                .ToListAsync();

            return Json(new { movies, series });
        }

        // AJAX endpoint за lazy loading на съдържание
        [HttpGet]
        public async Task<IActionResult> LoadMoreMovies(int skip = 0, int take = 10)
        {
            var movies = await _context.Movies
                .OrderByDescending(m => m.IMDBScore)
                .Skip(skip)
                .Take(take)
                .Select(m => new
                {
                    id = m.Id,
                    title = m.Title,
                    year = m.ReleaseYear,
                    rating = m.IMDBScore,
                    image = m.ImageUrl
                })
                .ToListAsync();

            return Json(movies);
        }

        public IActionResult NewsSection()
        {
            var topGenres = _context.Genres
                                    .OrderByDescending(g => g.ContentGenres.Count(c => c.Content.ReleaseYear == 2025))
                                    .Take(3)
                                    .ToList();

            var yearActor = _context.Actors
                                    .Include(a => a.ContentActors)
                                    .ThenInclude(ca => ca.Content)
                                    .OrderByDescending(a => a.ContentActors.Count(ca => ca.Content.ReleaseYear == 2025))
                                    .FirstOrDefault();

            var yearActorRoles = yearActor?.ContentActors
                                          .Where(ca => ca.Content.ReleaseYear == 2025)
                                          .Select(ca => ca.Content)
                                          .ToList() ?? new List<Content>();

            var awardedContent = _context.Movies
                                         .Where(c => c.ReleaseYear == 2025)
                                         .OrderByDescending(c => c.IMDBScore)
                                         .FirstOrDefault();

            var model = new HomeNewsViewModel
            {
                TopGenres = topGenres,
                YearActor = yearActor,
                YearActorRoles = yearActorRoles,
                AwardedContent = awardedContent
            };

            return PartialView("_NewsSection", model);
        }

    }
}