namespace RateFlix.Core.ViewModels
{
    public class SeriesCardViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public int ReleaseYear { get; set; }
        public double IMDBScore { get; set; }
        public string? ImageUrl { get; set; }
        public int TotalSeasons { get; set; }
        public List<string> Genres { get; set; } = new List<string>();
    }
}
