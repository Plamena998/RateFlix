namespace RateFlix.Core.ViewModels
{
    public class FavoriteContentViewModel
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public double IMDBScore { get; set; }
        public int? ReleaseYear { get; set; }
        public DateTime DateAdded { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public List<string> Genres { get; set; } = new();

    }
}
