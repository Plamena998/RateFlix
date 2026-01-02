using RateFlix.Data.Models;

namespace RateFlix.Models.ViewModels
{
    public class HomeViewModel
    {
        public HomeNewsViewModel News { get; set; }
        public List<Movie> TopMovies { get; set; } = new();
        public List<Series> TopSeries { get; set; } = new();
        public List<Movie> TrendingMovies { get; set; } = new();
        public List<Series> NewReleases { get; set; } = new();
    }
}