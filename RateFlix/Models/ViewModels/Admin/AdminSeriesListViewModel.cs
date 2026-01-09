namespace RateFlix.Models.ViewModels.Admin
{
    public class AdminSeriesListViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int ReleaseYear { get; set; }
        public double IMDBScore { get; set; }
        public string DirectorName { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();
        public int TotalSeasons { get; set; }
    }
}
