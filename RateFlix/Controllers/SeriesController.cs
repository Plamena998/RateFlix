using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateFlix.Data;
using RateFlix.Data.Models;
using RateFlix.Models.ViewModels;

namespace RateFlix.Controllers
{
    public class SeriesController : Controller
    {
        private readonly AppDbContext _context;

        public SeriesController(AppDbContext context)
        {
            _context = context;
        }

        // ============================
        // INDEX (Main Series Page)
        // ============================
        public async Task<IActionResult> Index(
            string? search,
            int? genreId,
            int? year,
            string? sortBy,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Series
                .Include(s => s.ContentGenres)
                    .ThenInclude(cg => cg.Genre)
                .Include(s => s.Director)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.Title.Contains(search));

            if (genreId.HasValue)
                query = query.Where(s => s.ContentGenres.Any(cg => cg.GenreId == genreId));

            if (year.HasValue)
                query = query.Where(s => s.ReleaseYear == year);

            query = sortBy switch
            {
                "title" => query.OrderBy(s => s.Title),
                "year_desc" => query.OrderByDescending(s => s.ReleaseYear),
                "year_asc" => query.OrderBy(s => s.ReleaseYear),
                "rating_asc" => query.OrderBy(s => s.IMDBScore),
                _ => query.OrderByDescending(s => s.IMDBScore)
            };

            var totalSeries = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalSeries / (double)pageSize);

            var series = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var genres = await _context.Genres
                .OrderBy(g => g.Name)
                .ToListAsync();

            var model = new SeriesIndexViewModel
            {
                Series = series,
                Genres = genres,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalSeries = totalSeries,
                PageSize = pageSize,
                Search = search,
                SelectedGenreId = genreId,
                SelectedYear = year,
                SortBy = sortBy
            };

            return View(model);
        }

        // ============================
        // AJAX PAGINATION
        // ============================
        [HttpGet]
        public async Task<IActionResult> LoadPage(
            string? search,
            int? genreId,
            int? year,
            string? sortBy,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Series.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(s => s.Title.Contains(search));

            if (genreId.HasValue)
                query = query.Where(s => s.ContentGenres.Any(cg => cg.GenreId == genreId));

            if (year.HasValue)
                query = query.Where(s => s.ReleaseYear == year);

            query = sortBy switch
            {
                "title" => query.OrderBy(s => s.Title),
                "year_desc" => query.OrderByDescending(s => s.ReleaseYear),
                "year_asc" => query.OrderBy(s => s.ReleaseYear),
                "rating_asc" => query.OrderBy(s => s.IMDBScore),
                _ => query.OrderByDescending(s => s.IMDBScore)
            };

            var totalSeries = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalSeries / (double)pageSize);

            var series = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new
                {
                    id = s.Id,
                    title = s.Title,
                    year = s.ReleaseYear,
                    rating = s.IMDBScore,
                    image = s.ImageUrl,
                    totalSeasons = s.TotalSeasons
                })
                .ToListAsync();

            return Json(new
            {
                series,
                currentPage = page,
                totalPages,
                totalSeries
            });
        }

        // ============================
        // CONTENT MODAL (SERIES INFO)
        // ============================
        [HttpGet]
        public async Task<IActionResult> GetContentModal(int id)
        {
            var series = await _context.Series
                .Include(s => s.Director)
                .Include(s => s.ContentGenres)
                    .ThenInclude(cg => cg.Genre)
                .Include(s => s.ContentActors)
                    .ThenInclude(ca => ca.Actor)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (series == null)
                return NotFound();

            return PartialView("Components/_ContentModal", series);
        }

        // ============================
        // SEASONS & EPISODES MODAL
        // ============================
        [HttpGet]
        public async Task<IActionResult> GetSeasonsModal(int id)
        {
            var series = await _context.Series
                .Include(s => s.Seasons.OrderBy(se => se.SeasonNumber))
                    .ThenInclude(se => se.Episodes.OrderBy(ep => ep.EpisodeNumber))
                .FirstOrDefaultAsync(s => s.Id == id);

            if (series == null)
                return NotFound();

            return PartialView("Components/_SeasonsModal", series);
        }

        //// ============================
        //// DETAILS PAGE (OPTIONAL)
        //// ============================
        //public async Task<IActionResult> Details(int id)
        //{
        //    var series = await _context.Series
        //        .Include(s => s.Director)
        //        .Include(s => s.ContentGenres)
        //            .ThenInclude(cg => cg.Genre)
        //        .Include(s => s.ContentActors)
        //            .ThenInclude(ca => ca.Actor)
        //        .Include(s => s.Seasons.OrderBy(se => se.SeasonNumber))
        //            .ThenInclude(se => se.Episodes.OrderBy(ep => ep.EpisodeNumber))
        //        .FirstOrDefaultAsync(s => s.Id == id);

        //    if (series == null)
        //        return NotFound();

        //    return View(series);
        //}
    }
}
