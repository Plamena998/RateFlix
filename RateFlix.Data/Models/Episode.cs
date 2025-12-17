using RateFlix.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Data.Models
{
    public class Episode : IBaseEntity
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string Title { get; set; } = string.Empty;
        public int SeasonId { get; set; }
        public Season Season { get; set; } = null!;
        public int EpisodeNumber { get; set; }
        public string? TrailerUrl { get; set; }
        public double IMDBScore { get; set; }
        public double MetaScore { get; set; }
        public int? Duration { get; set; } // Duration in minutes
    }
}
