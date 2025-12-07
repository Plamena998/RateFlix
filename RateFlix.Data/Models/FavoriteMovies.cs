using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Infrastructure
{
    public class FavoriteMovies //join table movie-user
    {
        public string UserId { get; set; } 
        public AppUser User { get; set; }

        public int MovieId { get; set; }
        public Movie Movie { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    }
}
