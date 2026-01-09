using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RateFlix.Data;
using RateFlix.Data.Models;
using RateFlix.Infrastructure;
using RateFlix.Models.ViewModels.Admin;

namespace RateFlix.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminSeriesController : Controller
    {
        private readonly AppDbContext _context;

        public AdminSeriesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AdminSeries
        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            int pageSize = 20;
            var query = _context.Series
                .Include(s => s.Director)
                .Include(s => s.ContentGenres)
                    .ThenInclude(cg => cg.Genre)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s => s.Title.Contains(search));
            }

            var totalSeries = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalSeries / (double)pageSize);

            var series = await query
                .OrderByDescending(s => s.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new AdminSeriesListViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    ImageUrl = s.ImageUrl,
                    ReleaseYear = s.ReleaseYear,
                    IMDBScore = s.IMDBScore,
                    DirectorName = s.Director.Name,
                    Genres = s.ContentGenres.Select(cg => cg.Genre.Name).ToList(),
                    TotalSeasons = s.TotalSeasons
                })
                .ToListAsync();

            var model = new AdminSeriesIndexViewModel
            {
                Series = series,
                Search = search,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalSeries = totalSeries
            };

            return View(model);
        }

        // GET: AdminSeries/Create
        public async Task<IActionResult> Create()
        {
            var model = new AdminSeriesFormViewModel
            {
                Directors = await GetDirectorsSelectList(),
                Genres = await GetGenresSelectList(),
                Actors = await GetActorsSelectList()
            };

            return View(model);
        }

        // POST: AdminSeries/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminSeriesFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var series = new Series
                {
                    Title = model.Title,
                    Description = model.Description,
                    ReleaseYear = model.ReleaseYear,
                    TotalSeasons = model.TotalSeasons,
                    IMDBScore = model.IMDBScore,
                    MetaScore = model.MetaScore,
                    ImageUrl = model.ImageUrl,
                    TrailerUrl = model.TrailerUrl,
                    DirectorId = model.DirectorId,
                    ContentType = "Series",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Series.Add(series);
                await _context.SaveChangesAsync();

                // Add genres
                if (model.SelectedGenreIds != null && model.SelectedGenreIds.Any())
                {
                    foreach (var genreId in model.SelectedGenreIds)
                    {
                        _context.ContentGenres.Add(new ContentGenre
                        {
                            ContentId = series.Id,
                            GenreId = genreId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                // Add actors
                if (model.SelectedActorIds != null && model.SelectedActorIds.Any())
                {
                    foreach (var actorId in model.SelectedActorIds)
                    {
                        _context.ContentActors.Add(new ContentActor
                        {
                            ContentId = series.Id,
                            ActorId = actorId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Series created successfully!";
                return RedirectToAction(nameof(Index));
            }

            model.Directors = await GetDirectorsSelectList();
            model.Genres = await GetGenresSelectList();
            model.Actors = await GetActorsSelectList();
            return View(model);
        }

        // GET: AdminSeries/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var series = await _context.Series
                .Include(s => s.ContentGenres)
                .Include(s => s.ContentActors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (series == null)
                return NotFound();

            var model = new AdminSeriesFormViewModel
            {
                Id = series.Id,
                Title = series.Title,
                Description = series.Description,
                ReleaseYear = series.ReleaseYear,
                TotalSeasons = series.TotalSeasons,
                IMDBScore = series.IMDBScore,
                MetaScore = series.MetaScore,
                ImageUrl = series.ImageUrl,
                TrailerUrl = series.TrailerUrl,
                DirectorId = series.DirectorId,
                SelectedGenreIds = series.ContentGenres.Select(cg => cg.GenreId).ToList(),
                SelectedActorIds = series.ContentActors.Select(ca => ca.ActorId).ToList(),
                Directors = await GetDirectorsSelectList(),
                Genres = await GetGenresSelectList(),
                Actors = await GetActorsSelectList()
            };

            return View(model);
        }

        // POST: AdminSeries/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminSeriesFormViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var series = await _context.Series
                    .Include(s => s.ContentGenres)
                    .Include(s => s.ContentActors)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (series == null)
                    return NotFound();

                // Update series properties
                series.Title = model.Title;
                series.Description = model.Description;
                series.ReleaseYear = model.ReleaseYear;
                series.TotalSeasons = model.TotalSeasons;
                series.IMDBScore = model.IMDBScore;
                series.MetaScore = model.MetaScore;
                series.ImageUrl = model.ImageUrl;
                series.TrailerUrl = model.TrailerUrl;
                series.DirectorId = model.DirectorId;
                series.UpdatedAt = DateTime.UtcNow;

                // Update genres
                _context.ContentGenres.RemoveRange(series.ContentGenres);
                if (model.SelectedGenreIds != null && model.SelectedGenreIds.Any())
                {
                    foreach (var genreId in model.SelectedGenreIds)
                    {
                        _context.ContentGenres.Add(new ContentGenre
                        {
                            ContentId = series.Id,
                            GenreId = genreId
                        });
                    }
                }

                // Update actors
                _context.ContentActors.RemoveRange(series.ContentActors);
                if (model.SelectedActorIds != null && model.SelectedActorIds.Any())
                {
                    foreach (var actorId in model.SelectedActorIds)
                    {
                        _context.ContentActors.Add(new ContentActor
                        {
                            ContentId = series.Id,
                            ActorId = actorId
                        });
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Series updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            model.Directors = await GetDirectorsSelectList();
            model.Genres = await GetGenresSelectList();
            model.Actors = await GetActorsSelectList();
            return View(model);
        }

        // POST: AdminSeries/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var series = await _context.Series.FindAsync(id);
            if (series == null)
                return NotFound();

            _context.Series.Remove(series);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Series deleted successfully!";
            return RedirectToAction(nameof(Index));
        }

        // Helper methods
        private async Task<List<SelectListItem>> GetDirectorsSelectList()
        {
            return await _context.Directors
                .Select(d => new SelectListItem
                {
                    Value = d.Id.ToString(),
                    Text = d.Name
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetGenresSelectList()
        {
            return await _context.Genres
                .Select(g => new SelectListItem
                {
                    Value = g.Id.ToString(),
                    Text = g.Name
                })
                .ToListAsync();
        }

        private async Task<List<SelectListItem>> GetActorsSelectList()
        {
            return await _context.Actors
                .Select(a => new SelectListItem
                {
                    Value = a.Id.ToString(),
                    Text = a.Name
                })
                .ToListAsync();
        }
    }
}