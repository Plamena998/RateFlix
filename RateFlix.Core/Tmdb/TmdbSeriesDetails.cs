using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Tmdb
{
    public class TmdbSeriesDetails
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Overview { get; set; } = string.Empty;
        public string First_air_date { get; set; } = string.Empty;
        public string? Poster_path { get; set; }
        public double Vote_average { get; set; }
        public int Number_of_seasons { get; set; }
        public int Number_of_episodes { get; set; }
        public List<TmdbGenre> Genres { get; set; } = new();
        public List<TmdbCreator> Created_by { get; set; } = new();
        public List<TmdbSeasonInfo> Seasons { get; set; } = new();
    }
}
