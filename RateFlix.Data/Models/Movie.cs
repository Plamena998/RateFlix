using RateFlix.Infrastructure.Base;

namespace RateFlix.Infrastructure
{
    public class Movie : IBaseEntity
    {
        public int Id { get ; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public DateTime DeletedAt { get; set; }
        public string Title { get; set; }
        public int ReleaseYear { get; set; }
        public string Description { get; set; }
        public decimal IMDBScore { get; set; }
        public decimal MetaScore { get; set; }
        public int DirectorId { get; set; }
        public Director Director { get; set; }
        public ICollection<MovieGenre> MovieGenre { get; set; }
        public ICollection<FavoriteMovies> FavoriteMovies { get; set; }
        public ICollection<Review> Reviews { get; set; }

    }
}
