namespace RateFlix.Core.ViewModels
{
    public class ContentViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string ImageUrl { get; set; }
        public int ReleaseYear { get; set; }
        public double IMDBScore { get; set; }
        public string ContentType { get; set; } // "Movie" or "Series"
        public string Description { get; set; }
        public string TrailerUrl { get; set; }

        // Director/Creator
        public int DirectorId { get; set; }
        public string DirectorName { get; set; }
        public string DirectorImageUrl { get; set; }

        // Movie-specific
        public int? Duration { get; set; }

        // Series-specific
        public int? TotalSeasons { get; set; }

        // Genres
        public List<GenreViewModel> Genres { get; set; } = new List<GenreViewModel>();

        // Actors
        public List<ActorViewModel> Actors { get; set; } = new List<ActorViewModel>();
        public List<ReviewViewModel> TopReviews { get; set; } = new List<ReviewViewModel>();


    }
}
