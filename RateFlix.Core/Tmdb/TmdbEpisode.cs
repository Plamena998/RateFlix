using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Tmdb
{
    public class TmdbEpisode
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public int Episode_number { get; set; }
        public int Season_number { get; set; }
        public string? Air_date { get; set; }
        public int Runtime { get; set; }
        public string? Still_path { get; set; }
        public double Vote_average { get; set; }
    }
}
