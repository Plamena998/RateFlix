using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Tmdb
{
    public class TmdbApiResponse<T>
    {
        public int Page { get; set; }
        public List<T> Results { get; set; } = new List<T>();
        public int Total_pages { get; set; }
        public int Total_results { get; set; }
    }
}
