using RateFlix.Core.Models;
using RateFlix.Core.ViewModels;

namespace RateFlix.Core.ViewModels
{
    public class HomeViewModel
    {
        public HomeNewsViewModel News { get; set; }
        public List<MovieCardViewModel> TopMovies { get; set; } = new();
        public List<SeriesCardViewModel> TopSeries { get; set; } = new();
        public List<MovieCardViewModel> TrendingMovies { get; set; } = new();
        public List<SeriesCardViewModel> NewReleases { get; set; } = new();
    }
}