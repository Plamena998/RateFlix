using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Tmdb
{
    public class TmdbApiResponseSeries<T>
    {
        public int Page { get; set; }
        public List<TmdbSeries> Results { get; set; } = new List<TmdbSeries>();
        public int Total_pages { get; set; }
        public int Total_results { get; set; }
    }
}
