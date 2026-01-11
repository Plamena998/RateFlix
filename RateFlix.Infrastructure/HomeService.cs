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
            return new HomeViewModel
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
        }

        public async Task<SearchResultsViewModel> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new SearchResultsViewModel();

            return new SearchResultsViewModel
            {
                Movies = await _context.Movies
                    .Where(m => m.Title.Contains(query))
                    .Take(5)
                    .Select(m => new SearchItemViewModel
                    {
                        Id = m.Id,
                        Title = m.Title,
                        ReleaseYear = m.ReleaseYear,
                        IMDBScore = m.IMDBScore,
                        ImageUrl = m.ImageUrl,
                        Type = "movie"
                    })
                    .ToListAsync(),

                Series = await _context.Series
                    .Where(s => s.Title.Contains(query))
                    .Take(5)
                    .Select(s => new SearchItemViewModel
                    {
                        Id = s.Id,
                        Title = s.Title,
                        ReleaseYear = s.ReleaseYear,
                        IMDBScore = s.IMDBScore,
                        ImageUrl = s.ImageUrl,
                        Type = "series"
                    })
                    .ToListAsync()
            };
        }

        public async Task<List<MovieCardViewModel>> LoadMoreMoviesAsync(int skip, int take)
        {
            return await _context.Movies
                .OrderByDescending(m => m.IMDBScore)
                .Skip(skip)
                .Take(take)
                .Select(m => new MovieCardViewModel
                {
                    Id = m.Id,
                    Title = m.Title,
                    ReleaseYear = m.ReleaseYear,
                    IMDBScore = m.IMDBScore,
                    ImageUrl = m.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<HomeNewsViewModel> GetNewsSectionAsync()
        {
            var topGenres = await _context.Genres
                .OrderByDescending(g => g.ContentGenres.Count(c => c.Content.ReleaseYear == 2025))
                .Take(3)
                .ToListAsync();

            var yearActor = await _context.Actors
                .Include(a => a.ContentActors)
                .ThenInclude(ca => ca.Content)
                .OrderByDescending(a => a.ContentActors.Count(ca => ca.Content.ReleaseYear == 2025))
                .FirstOrDefaultAsync();

            var yearActorRoles = yearActor?.ContentActors
                .Where(ca => ca.Content.ReleaseYear == 2025)
                .Select(ca => ca.Content)
                .ToList() ?? new List<Content>();

            var awardedContent = await _context.Movies
                .Where(c => c.ReleaseYear == 2025)
                .OrderByDescending(c => c.IMDBScore)
                .FirstOrDefaultAsync();

            return new HomeNewsViewModel
            {
                TopGenres = topGenres,
                YearActor = yearActor,
                YearActorRoles = yearActorRoles,
                AwardedContent = awardedContent
            };
        }
    }
}
