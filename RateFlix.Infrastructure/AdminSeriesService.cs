using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RateFlix.Core.Models;
using RateFlix.Core.ViewModels.Admin;
using RateFlix.Data;
using RateFlix.Services.Interfaces;

namespace RateFlix.Services
{
    public class AdminSeriesService : IAdminSeriesService
    {
        private readonly AppDbContext _context;

        public AdminSeriesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<AdminSeriesIndexViewModel> GetSeriesAsync(string search = "", int page = 1, int pageSize = 20)
        {
            var query = _context.Series
                .Include(s => s.Director)
                .Include(s => s.ContentGenres)
                    .ThenInclude(cg => cg.Genre)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
                query = query.Where(s => s.Title.Contains(search));

            var totalSeries = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalSeries / (double)pageSize);

            var seriesList = await query
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

            return new AdminSeriesIndexViewModel
            {
                Series = seriesList,
                Search = search,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalSeries = totalSeries
            };
        }

        public async Task<AdminSeriesFormViewModel> GetSeriesFormAsync(int? id = null)
        {
            AdminSeriesFormViewModel model;

            if (id.HasValue)
            {
                var series = await _context.Series
                    .Include(s => s.ContentGenres)
                    .Include(s => s.ContentActors)
                    .FirstOrDefaultAsync(s => s.Id == id.Value);

                if (series == null) return null!;

                model = new AdminSeriesFormViewModel
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
                    SelectedActorIds = series.ContentActors.Select(ca => ca.ActorId).ToList()
                };
            }
            else
            {
                model = new AdminSeriesFormViewModel();
            }

            model.Directors = await GetDirectorsSelectListAsync();
            model.Genres = await GetGenresSelectListAsync();
            model.Actors = await GetActorsSelectListAsync();

            return model;
        }

        public async Task<bool> CreateSeriesAsync(AdminSeriesFormViewModel model)
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

            await AddGenresAndActorsAsync(series.Id, model.SelectedGenreIds, model.SelectedActorIds);

            return true;
        }

        public async Task<bool> UpdateSeriesAsync(AdminSeriesFormViewModel model)
        {
            var series = await _context.Series
                .Include(s => s.ContentGenres)
                .Include(s => s.ContentActors)
                .FirstOrDefaultAsync(s => s.Id == model.Id);

            if (series == null) return false;

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

            _context.ContentGenres.RemoveRange(series.ContentGenres);
            _context.ContentActors.RemoveRange(series.ContentActors);

            await AddGenresAndActorsAsync(series.Id, model.SelectedGenreIds, model.SelectedActorIds);

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteSeriesAsync(int id)
        {
            var series = await _context.Series.FindAsync(id);
            if (series == null) return false;

            _context.Series.Remove(series);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task AddGenresAndActorsAsync(int contentId, List<int>? genreIds, List<int>? actorIds)
        {
            if (genreIds != null && genreIds.Any())
            {
                foreach (var genreId in genreIds)
                {
                    _context.ContentGenres.Add(new ContentGenre
                    {
                        ContentId = contentId,
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
                        ContentId = contentId,
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
