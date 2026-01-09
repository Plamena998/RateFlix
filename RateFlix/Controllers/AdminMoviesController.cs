using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RateFlix.Data;
using RateFlix.Data.Models;
using RateFlix.Infrastructure;
using RateFlix.Models.ViewModels;
using RateFlix.Models.ViewModels.Admin;

namespace RateFlix.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminMoviesController : Controller
    {
        private readonly AppDbContext _context;

        public AdminMoviesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: AdminMovies
        public async Task<IActionResult> Index(string search = "", int page = 1)
        {
            int pageSize = 20;
            var query = _context.Movies
                .Include(m => m.Director)
                .Include(m => m.ContentGenres)
                    .ThenInclude(cg => cg.Genre)
                .AsQueryable();

            // Search filter
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(m => m.Title.Contains(search));
            }

            var totalMovies = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalMovies / (double)pageSize);

            var movies = await query
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(m => new AdminMovieListViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    ImageUrl = m.ImageUrl,
                    ReleaseYear = m.ReleaseYear,
                    IMDBScore = m.IMDBScore,
                    DirectorName = m.Director.Name,
                    Genres = m.ContentGenres.Select(cg => cg.Genre.Name).ToList(),
                    Duration = m.Duration
                })
                .ToListAsync();

            var model = new AdminMoviesIndexViewModel
            {
                Movies = movies,
                Search = search,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalMovies = totalMovies
            };

            return View(model);
        }

        // GET: AdminMovies/Create
        public async Task<IActionResult> Create()
        {
            var model = new AdminMovieFormViewModel
            {
                Directors = await GetDirectorsSelectList(),
                Genres = await GetGenresSelectList(),
                Actors = await GetActorsSelectList()
            };

            return View(model);
        }

        // POST: AdminMovies/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminMovieFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var movie = new Movie
                {
                    Title = model.Title,
                    Description = model.Description,
                    ReleaseYear = model.ReleaseYear,
                    Duration = model.Duration,
                    IMDBScore = model.IMDBScore,
                    MetaScore = model.MetaScore,
                    ImageUrl = model.ImageUrl,
                    TrailerUrl = model.TrailerUrl,
                    DirectorId = model.DirectorId,
                    ContentType = "Movie",
                    CreatedAt = DateTime.UtcNow
                };

                _context.Movies.Add(movie);
                await _context.SaveChangesAsync();

                // Add genres
                if (model.SelectedGenreIds != null && model.SelectedGenreIds.Any())
                {
                    foreach (var genreId in model.SelectedGenreIds)
                    {
                        _context.ContentGenres.Add(new ContentGenre
                        {
                            ContentId = movie.Id,
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
                            ContentId = movie.Id,
                            ActorId = actorId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = "Movie created successfully!";
                return RedirectToAction(nameof(Index));
            }

            // Reload dropdowns if validation fails
            model.Directors = await GetDirectorsSelectList();
            model.Genres = await GetGenresSelectList();
            model.Actors = await GetActorsSelectList();
            return View("Form", model);
        }

        // GET: AdminMovies/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var movie = await _context.Movies
                .Include(m => m.ContentGenres)
                .Include(m => m.ContentActors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
                return NotFound();

            var model = new AdminMovieFormViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                Description = movie.Description,
                ReleaseYear = movie.ReleaseYear,
                Duration = movie.Duration,
                IMDBScore = movie.IMDBScore,
                MetaScore = movie.MetaScore,
                ImageUrl = movie.ImageUrl,
                TrailerUrl = movie.TrailerUrl,
                DirectorId = movie.DirectorId,
                SelectedGenreIds = movie.ContentGenres.Select(cg => cg.GenreId).ToList(),
                SelectedActorIds = movie.ContentActors.Select(ca => ca.ActorId).ToList(),
                Directors = await GetDirectorsSelectList(),
                Genres = await GetGenresSelectList(),
                Actors = await GetActorsSelectList()
            };

            return View(model);
        }

        // POST: AdminMovies/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminMovieFormViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                var movie = await _context.Movies
                    .Include(m => m.ContentGenres)
                    .Include(m => m.ContentActors)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (movie == null)
                    return NotFound();

                // Update movie properties
                movie.Title = model.Title;
                movie.Description = model.Description;
                movie.ReleaseYear = model.ReleaseYear;
                movie.Duration = model.Duration;
                movie.IMDBScore = model.IMDBScore;
                movie.MetaScore = model.MetaScore;
                movie.ImageUrl = model.ImageUrl;
                movie.TrailerUrl = model.TrailerUrl;
                movie.DirectorId = model.DirectorId;
                movie.UpdatedAt = DateTime.UtcNow;

                // Update genres
                _context.ContentGenres.RemoveRange(movie.ContentGenres);
                if (model.SelectedGenreIds != null && model.SelectedGenreIds.Any())
                {
                    foreach (var genreId in model.SelectedGenreIds)
                    {
                        _context.ContentGenres.Add(new ContentGenre
                        {
                            ContentId = movie.Id,
                            GenreId = genreId
                        });
                    }
                }

                // Update actors
                _context.ContentActors.RemoveRange(movie.ContentActors);
                if (model.SelectedActorIds != null && model.SelectedActorIds.Any())
                {
                    foreach (var actorId in model.SelectedActorIds)
                    {
                        _context.ContentActors.Add(new ContentActor
                        {
                            ContentId = movie.Id,
                            ActorId = actorId
                        });
                    }
                }

                await _context.SaveChangesAsync();

                TempData["Success"] = "Movie updated successfully!";
                return RedirectToAction(nameof(Index));
            }

            model.Directors = await GetDirectorsSelectList();
            model.Genres = await GetGenresSelectList();
            model.Actors = await GetActorsSelectList();
            return View("Form", model);
        }

        // POST: AdminMovies/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
                return NotFound();

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Movie deleted successfully!";
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