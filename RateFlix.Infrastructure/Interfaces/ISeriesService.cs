using RateFlix.Core.Models;
using RateFlix.Core.ViewModels;
namespace RateFlix.Services.Interfaces
{
    public interface ISeriesService
    {
        Task<SeriesIndexViewModel> GetSeriesIndexAsync(
            string? search,
            int? genreId,
            int? year,
            string? sortBy,
            int page = 1,
            int pageSize = 20);

        Task<(List<SeriesCardViewModel> Series, int CurrentPage, int TotalPages, int TotalSeries)>
            LoadSeriesPageAsync(
                string? search,
                int? genreId,
                int? year,
                string? sortBy,
                int page = 1,
                int pageSize = 20);

        Task<ContentViewModel?> GetSeriesWithDetailsAsync(int id);
        Task<SeasonsViewModel?> GetSeriesWithSeasonsAsync(int id);
    }
}
