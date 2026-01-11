using RateFlix.Infrastructure;
using RateFlix.Infrastructure.Base;

namespace RateFlix.Core.Models
{
    public class Movie : Content
    {
        public int Duration { get; set; } // Duration in minutes
        public int DirectorId { get; set; }
        public Director Director { get; set; }

    }
}
