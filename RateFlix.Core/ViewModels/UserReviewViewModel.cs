namespace RateFlix.Core.ViewModels
{
    public class UserReviewViewModel
    {
        public int ContentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Rating { get; set; } // 1-10 or whatever your range is
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public double IMDBScore { get; set; }
        public string ContentType { get; set; } = string.Empty; // "Movie" or "Series"

    }
}
