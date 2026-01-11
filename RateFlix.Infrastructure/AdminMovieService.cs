using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RateFlix.Core.Models;
using RateFlix.Core.ViewModels.Admin;
using RateFlix.Data;
using RateFlix.Services.Interfaces;

namespace RateFlix.Services
{
    public class AdminMovieService : IAdminMovieService
    {
        private readonly AppDbContext _context;

        public AdminMovieService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminMoviesIndexViewModel> GetMoviesAsync(string search = "", int page = 1, int pageSize = 20)
        {
            var query = _context.Movies
                .Include(m => m.Director)
                .Include(m => m.ContentGenres)
                    .ThenInclude(cg => cg.Genre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(m => m.Title.Contains(search));

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

            return new AdminMoviesIndexViewModel
            {
                Movies = movies,
                Search = search,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalMovies = totalMovies
            };
        }

        public async Task<AdminMovieFormViewModel> GetMovieFormAsync(int? id = null)
        {
            AdminMovieFormViewModel model;

            if (id.HasValue)
            {
                var movie = await _context.Movies
                    .Include(m => m.ContentGenres)
                    .Include(m => m.ContentActors)
                    .FirstOrDefaultAsync(m => m.Id == id.Value);

                if (movie == null) return null!;

                model = new AdminMovieFormViewModel
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
                    SelectedActorIds = movie.ContentActors.Select(ca => ca.ActorId).ToList()
                };
            }
            else
            {
                model = new AdminMovieFormViewModel();
            }

            model.Directors = await GetDirectorsSelectListAsync();
            model.Genres = await GetGenresSelectListAsync();
            model.Actors = await GetActorsSelectListAsync();

            return model;
        }

        public async Task<bool> CreateMovieAsync(AdminMovieFormViewModel model)
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

            await AddGenresAndActorsAsync(movie.Id, model.SelectedGenreIds, model.SelectedActorIds);

            return true;
        }

        public async Task<bool> UpdateMovieAsync(AdminMovieFormViewModel model)
        {
            var movie = await _context.Movies
                .Include(m => m.ContentGenres)
                .Include(m => m.ContentActors)
                .FirstOrDefaultAsync(m => m.Id == model.Id);

            if (movie == null) return false;

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

            _context.ContentGenres.RemoveRange(movie.ContentGenres);
            _context.ContentActors.RemoveRange(movie.ContentActors);

            await AddGenresAndActorsAsync(movie.Id, model.SelectedGenreIds, model.SelectedActorIds);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteMovieAsync(int id)
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null) return false;

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task AddGenresAndActorsAsync(int movieId, List<int>? genreIds, List<int>? actorIds)
        {
            if (genreIds != null && genreIds.Any())
            {
                foreach (var genreId in genreIds)
                {
                    _context.ContentGenres.Add(new ContentGenre
                    {
                        ContentId = movieId,
                        GenreId = genreId
                    });
                }
            }

            if (actorIds != null && actorIds.Any())
            {
                foreach (var actorId in actorIds)
                {
                    _context.ContentActors.Add(new ContentActor
                    {
                        ContentId = movieId,
                        ActorId = actorId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<SelectListItem>> GetDirectorsSelectListAsync()
        {
            return await _context.Directors
                .Select(d => new SelectListItem { Value = d.Id.ToString(), Text = d.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetGenresSelectListAsync()
        {
            return await _context.Genres
                .Select(g => new SelectListItem { Value = g.Id.ToString(), Text = g.Name })
                .ToListAsync();
        }

        public async Task<List<SelectListItem>> GetActorsSelectListAsync()
        {
            return await _context.Actors
                .Select(a => new SelectListItem { Value = a.Id.ToString(), Text = a.Name })
                .ToListAsync();
        }
    }
}
