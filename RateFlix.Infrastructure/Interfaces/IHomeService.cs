using RateFlix.Core.ViewModels;

namespace RateFlix.Services.Interfaces
{
    public interface IHomeService
    {
        Task<HomeViewModel> GetHomeAsync();
        Task<SearchResultsViewModel> SearchAsync(string query);
        Task<List<MovieCardViewModel>> LoadMoreMoviesAsync(int skip, int take);
        Task<HomeNewsViewModel> GetNewsSectionAsync();
    }
}
