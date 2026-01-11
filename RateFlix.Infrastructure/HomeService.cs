using Microsoft.EntityFrameworkCore;
using RateFlix.Core.Models;
using RateFlix.Core.ViewModels;
using RateFlix.Data;
using RateFlix.Services.Interfaces;

namespace RateFlix.Services
{
    public class HomeService : IHomeService
    {
        private readonly AppDbContext _context;

        public HomeService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<HomeViewModel> GetHomeAsync()
        {
            var topMovies = await _context.Movies
               .Include(s => s.ContentGenres).ThenInclude(cg => cg.Genre)
               .OrderByDescending(s => s.IMDBScore)
               .Take(10)
               .Select(s => new MovieCardViewModel
               {
                   Id = s.Id,
                   Title = s.Title,
                   ReleaseYear = s.ReleaseYear,
                   IMDBScore = s.IMDBScore,
                   ImageUrl = s.ImageUrl,
                   Duration = s.Duration,
                   Genres = s.ContentGenres.Take(2).Select(cg => cg.Genre.Name).ToList()
               })
               .ToListAsync();

            var trendingMovies = await _context.Movies
                .Include(m => m.ContentGenres).ThenInclude(cg => cg.Genre)
                 .OrderByDescending(m => m.ReleaseYear)
                 .ThenByDescending(m => m.IMDBScore)
                .Take(10)
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

            var topRatedSeries = await _context.Series
                .Include(s => s.ContentGenres).ThenInclude(cg => cg.Genre)
                .OrderByDescending(s => s.IMDBScore)
                .Take(10)
                .Select(s => new SeriesCardViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    ReleaseYear = s.ReleaseYear,
                    IMDBScore = s.IMDBScore,
                    ImageUrl = s.ImageUrl,
                    TotalSeasons = s.TotalSeasons,
                    Genres = s.ContentGenres.Take(2).Select(cg => cg.Genre.Name).ToList()
                })
                .ToListAsync();

            return new HomeViewModel
            {
                TrendingMovies = trendingMovies,
                TopSeries = topRatedSeries,
                TopMovies = topMovies
               
            };
        }

        public async Task<SearchResultsViewModel> SearchAsync(string query)
        {
            var movies = await _context.Movies
                .Where(m => m.Title.Contains(query))
                .Take(5)
                .Select(m => new SearchItemViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    ImageUrl = m.ImageUrl,
                    ReleaseYear = m.ReleaseYear,
                    IMDBScore = m.IMDBScore,
                    Type = "Movie" 
                })
                .ToListAsync();

            var series = await _context.Series
                .Where(s => s.Title.Contains(query))
                .Take(5)
                .Select(s => new SearchItemViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    ImageUrl = s.ImageUrl,
                    ReleaseYear = s.ReleaseYear,
                    IMDBScore = s.IMDBScore,
                    Type = "Series" 
                })
                .ToListAsync();

            return new SearchResultsViewModel
            {
                Movies = movies,
                Series = series
            };
        }

        public async Task<List<MovieCardViewModel>> LoadMoreMoviesAsync(int skip, int take)
        {
            return await _context.Movies
                .Include(m => m.ContentGenres).ThenInclude(cg => cg.Genre)
                .OrderByDescending(m => m.IMDBScore)
                .Skip(skip)
                .Take(take)
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
        }

        public async Task<HomeNewsViewModel> GetNewsSectionAsync()
        {
            var topGenres = await _context.Genres
                .Select(g => new GenreStatViewModel
                {
                    Id = g.Id,
                    Name = g.Name,
                    Count = g.ContentGenres.Count
                })
                .OrderByDescending(g => g.Count)
                .Take(3)
                .ToListAsync();

            var currentYear = DateTime.UtcNow.Year-1;

            var yearActor = await _context.Actors
                .Select(a => new
                {
                    Actor = new ActorViewModel
                    {
                        Id = a.Id,
                        Name = a.Name,
                        ImageUrl = a.ImageUrl
                    },
                    RolesCount = a.ContentActors.Count(ca =>
                        ca.Content.ReleaseYear == currentYear)
                })
                .Where(a => a.RolesCount > 0) 
                .OrderByDescending(a => a.RolesCount)
                .FirstOrDefaultAsync();

            List<ContentViewModel> yearActorRoles = new List<ContentViewModel>();

            if (yearActor != null)
            {
                yearActorRoles = await _context.ContentActors
                    .Where(ca => ca.ActorId == yearActor.Actor.Id &&
                        ca.Content.ReleaseYear == currentYear) 
                    .Select(ca => new ContentViewModel
                    {
                        Id = ca.ContentId,
                        Title = ca.Content.Title,
                        ImageUrl = ca.Content.ImageUrl,
                        IMDBScore = ca.Content.IMDBScore
                    })
                    .ToListAsync();
            }

            var awardedMovie = await _context.Movies
                .Where(m => m.ReleaseYear == currentYear)
                .OrderByDescending(m => m.IMDBScore)
                .Select(m => new ContentViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    ImageUrl = m.ImageUrl,
                    IMDBScore = m.IMDBScore
                })
                .FirstOrDefaultAsync();

            var awardedSeries = await _context.Series
                .Where(s => s.ReleaseYear == currentYear)
                .OrderByDescending(s => s.IMDBScore)
                .Select(s => new ContentViewModel
                {
                    Id = s.Id,
                    Title = s.Title,
                    ImageUrl = s.ImageUrl,
                    IMDBScore = s.IMDBScore
                })
                .FirstOrDefaultAsync();

            ContentViewModel? awardedContent = null;
            if (awardedMovie != null && awardedSeries != null)
            {
                awardedContent = awardedMovie.IMDBScore >= awardedSeries.IMDBScore ? awardedMovie : awardedSeries;
            }
            else
            {
                awardedContent = awardedMovie ?? awardedSeries;
            }

            return new HomeNewsViewModel
            {
                TopGenres = topGenres,
                YearActor = yearActor?.Actor,
                YearActorRoles = yearActorRoles,
                AwardedContent = awardedContent
            };
        }
    }
}