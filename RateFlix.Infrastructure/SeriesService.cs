using Microsoft.EntityFrameworkCore;
using RateFlix.Core.Models;
using RateFlix.Core.ViewModels;
using RateFlix.Data;
using RateFlix.Services.Interfaces;

namespace RateFlix.Services
{
    public class SeriesService : ISeriesService
    {
        private readonly AppDbContext _context;

        public SeriesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<SeriesIndexViewModel> GetSeriesIndexAsync(
            string? search,
            int? genreId,
            int? year,
            string? sortBy,
            int page = 1,
            int pageSize = 20)
        {
            var query = _context.Series
                .Include(s => s.ContentGenres).ThenInclude(cg => cg.Genre)
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

            var seriesList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

            var genres = await _context.Genres.OrderBy(g => g.Name).ToListAsync();

            return new SeriesIndexViewModel
            {
                Series = seriesList,
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
        }

        public async Task<(List<SeriesCardViewModel> Series, int CurrentPage, int TotalPages, int TotalSeries)>
     LoadSeriesPageAsync(string? search, int? genreId, int? year, string? sortBy, int page = 1, int pageSize = 20)
        {
            var query = _context.Series
                .Include(s => s.ContentGenres).ThenInclude(cg => cg.Genre)
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

            var seriesList = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

            return (seriesList, page, totalPages, totalSeries);
        }

        public async Task<ContentViewModel?> GetSeriesWithDetailsAsync(int id)
        {
            var series = await _context.Series
         .Include(s => s.Director)
         .Include(s => s.ContentGenres).ThenInclude(cg => cg.Genre)
         .Include(s => s.ContentActors).ThenInclude(ca => ca.Actor)
         .FirstOrDefaultAsync(s => s.Id == id);

            if (series == null) return null;

            return new ContentViewModel
            {
                Id = series.Id,
                Title = series.Title,
                ImageUrl = series.ImageUrl,
                ReleaseYear = series.ReleaseYear,
                IMDBScore = series.IMDBScore,
                ContentType = "Series",
                Description = series.Description,
                TrailerUrl = series.TrailerUrl,
                DirectorId = series.DirectorId,
                DirectorName = series.Director.Name,
                DirectorImageUrl = series.Director.ImageUrl,
                TotalSeasons = series.TotalSeasons,
                Genres = series.ContentGenres.Select(cg => new GenreViewModel
                {
                    Id = cg.Genre.Id,
                    Name = cg.Genre.Name
                }).ToList(),
                Actors = series.ContentActors.Select(ca => new ActorViewModel
                {
                    Id = ca.Actor.Id,
                    Name = ca.Actor.Name,
                    ImageUrl = ca.Actor.ImageUrl
                }).ToList()
            };
        }

        public async Task<SeasonsViewModel?> GetSeriesWithSeasonsAsync(int id)
        {
            var series = await _context.Series
          .Include(s => s.Seasons.OrderBy(se => se.SeasonNumber))
              .ThenInclude(se => se.Episodes.OrderBy(ep => ep.EpisodeNumber))
          .FirstOrDefaultAsync(s => s.Id == id);

            if (series == null) return null;

            return new SeasonsViewModel
            {
                SeriesId = series.Id,
                SeriesTitle = series.Title,
                Seasons = series.Seasons.OrderBy(s => s.SeasonNumber).Select(season => new SeasonViewModel
                {
                    Id = season.Id,
                    SeasonNumber = season.SeasonNumber,
                    IMDBScore = season.IMDBScore,
                    Episodes = season.Episodes.OrderBy(e => e.EpisodeNumber).Select(episode => new EpisodeViewModel
                    {
                        Id = episode.Id,
                        EpisodeNumber = episode.EpisodeNumber,
                        Title = episode.Title,
                        IMDBScore = episode.IMDBScore
                    }).ToList()
                }).ToList()
            };
        }
    }
}
