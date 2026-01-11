using Microsoft.AspNetCore.Identity;
using RateFlix.Infrastructure.Base;


namespace RateFlix.Core.Models
{
    public class AppUser : IdentityUser
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public ICollection<Review> Reviews { get; set; } = new HashSet<Review>();
        public ICollection<FavoriteContent> FavoriteMovies { get; set; } = new HashSet<FavoriteContent>();
    }
}
