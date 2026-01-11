using Microsoft.EntityFrameworkCore;
using RateFlix.Core.Models;
using RateFlix.Core.ViewModels;
using RateFlix.Data;
using RateFlix.Services.Interfaces;

namespace RateFlix.Services
{
    public class MovieService : IMovieService
    {
        private readonly AppDbContext _context;

        public MovieService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MoviesIndexViewModel> GetMoviesIndexAsync(
     string? search,
     int? genreId,
     int? year,
     string? sortBy,
     int page = 1,
     int pageSize = 20)
        {
            var query = _context.Movies
                .Include(m => m.ContentGenres).ThenInclude(cg => cg.Genre)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Title.Contains(search));

            if (genreId.HasValue)
                query = query.Where(m => m.ContentGenres.Any(cg => cg.GenreId == genreId.Value));

            if (year.HasValue)
                query = query.Where(m => m.ReleaseYear == year.Value);

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
                .Select(m => new MovieCardViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    ReleaseYear = m.ReleaseYear,
                    IMDBScore = m.IMDBScore,
                    ImageUrl = m.ImageUrl,
                    Duration = m.Duration,
                    Genres = m.ContentGenres.Take(2).Select(cg => cg.Genre.Name).ToList()
                })
                .ToListAsync();

            var genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();

            return new MoviesIndexViewModel
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
        }

        public async Task<(List<MovieCardViewModel> Movies, int CurrentPage, int TotalPages, int TotalMovies)>
            LoadMoviesPageAsync(string? search, int? genreId, int? year, string? sortBy, int page = 1, int pageSize = 20)
        {
            var query = _context.Movies.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(m => m.Title.Contains(search));

            if (genreId.HasValue)
                query = query.Where(m => m.ContentGenres.Any(cg => cg.GenreId == genreId.Value));

            if (year.HasValue)
                query = query.Where(m => m.ReleaseYear == year.Value);

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
                .Select(m => new MovieCardViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    ReleaseYear = m.ReleaseYear,
                    IMDBScore = m.IMDBScore,
                    ImageUrl = m.ImageUrl,
                    Duration = m.Duration
                })
                .ToListAsync();

            return (movies, page, totalPages, totalMovies);
        }

        public async Task<ContentViewModel?> GetMovieWithDetailsAsync(int id)
        {
            var movie = await _context.Movies
        .Include(m => m.Director)
        .Include(m => m.ContentGenres).ThenInclude(cg => cg.Genre)
        .Include(m => m.ContentActors).ThenInclude(ca => ca.Actor)
        .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null) return null;

            return new ContentViewModel
            {
                Id = movie.Id,
                Title = movie.Title,
                ImageUrl = movie.ImageUrl,
                ReleaseYear = movie.ReleaseYear,
                IMDBScore = movie.IMDBScore,
                ContentType = "Movie",
                Description = movie.Description,
                TrailerUrl = movie.TrailerUrl,
                DirectorId = movie.DirectorId,
                DirectorName = movie.Director.Name,
                DirectorImageUrl = movie.Director.ImageUrl,
                Duration = movie.Duration,
                Genres = movie.ContentGenres.Select(cg => new GenreViewModel
                {
                    Id = cg.Genre.Id,
                    Name = cg.Genre.Name
                }).ToList(),
                Actors = movie.ContentActors.Select(ca => new ActorViewModel
                {
                    Id = ca.Actor.Id,
                    Name = ca.Actor.Name,
                    ImageUrl = ca.Actor.ImageUrl
                }).ToList()
            };
        }
    }
}
