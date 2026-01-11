using RateFlix.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Models
{
    public class Season : IBaseEntity
    {
        public int Id {get; set;}
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int SeasonNumber { get; set; }
        public string? Description { get; set; }
        public int ReleaseYear { get; set; }
        public double IMDBScore { get; set; }
        public double MetaScore { get; set; }
        public int SeriesId { get; set; }
        public Series Series { get; set; } = null!;
        public ICollection<Episode> Episodes { get; set; } = new List<Episode>();

    }
}
