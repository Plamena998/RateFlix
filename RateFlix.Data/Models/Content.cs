using RateFlix.Infrastructure;
using RateFlix.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Data.Models
{
    public abstract class Content : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string Title { get; set; }
        public int ReleaseYear { get; set; }
        public string Description { get; set; } = string.Empty;
        public double IMDBScore { get; set; }
        public double MetaScore { get; set; }
        public string ContentType { get; set; } = string.Empty;
        public string TrailerUrl { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        
        public ICollection<Review>? Reviews { get; set; }
        public ICollection<FavoriteContent>? FavoriteMovies { get; set; }
        public ICollection<ContentGenre>? ContentGenres { get; set; }
        public ICollection<ContentActor> ContentActors { get; set; }

    }
}
