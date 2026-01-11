using RateFlix.Core.Models;
using RateFlix.Core.ViewModels;

namespace RateFlix.Services.Interfaces
{
    public interface IMovieService
    {
        Task<MoviesIndexViewModel> GetMoviesIndexAsync(
            string? search,
            int? genreId,
            int? year,
            string? sortBy,
            int page = 1,
            int pageSize = 20);

        Task<(List<MovieCardViewModel> Movies, int CurrentPage, int TotalPages, int TotalMovies)>
            LoadMoviesPageAsync(string? search, int? genreId, int? year, string? sortBy, int page = 1, int pageSize = 20);

        Task<ContentViewModel?> GetMovieWithDetailsAsync(int id);
    }
}
