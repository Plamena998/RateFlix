using RateFlix.Infrastructure;
using RateFlix.Infrastructure.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Models
{
    public class Series : Content
    {
        public int TotalSeasons { get; set; }
        public int DirectorId { get; set; }
        public Director Director { get; set; }
        public ICollection<Season> Seasons { get; set; } = new List<Season>();
    }
}
