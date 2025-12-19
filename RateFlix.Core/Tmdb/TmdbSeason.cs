using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Tmdb
{
    public class TmdbSeason
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public int Season_number { get; set; }
        public int Episode_count { get; set; }
        public string? Air_date { get; set; }
        public string? Poster_path { get; set; }
        public List<TmdbEpisode> Episodes { get; set; } = new();
    }
}
