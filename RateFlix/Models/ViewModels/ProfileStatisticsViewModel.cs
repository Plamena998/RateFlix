namespace RateFlix.Models.ViewModels
{
    public class ProfileStatisticsViewModel
    {
        public int TotalReviews { get; set; }
        public int TotalFavorites { get; set; }
        public double AverageRating { get; set; }
        public List<GenreStatViewModel> TopGenres { get; set; } = new();
        public int TotalMovies { get; set; }
        public int TotalSeries { get; set; }

    }
}
