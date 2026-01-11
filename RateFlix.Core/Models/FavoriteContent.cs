using RateFlix.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Models
{
    public class FavoriteContent //join table movie-user
    {
        public string UserId { get; set; } 
        public AppUser User { get; set; }
        public int ContentId { get; set; }
        public Content Content { get; set; }
        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    }
}
