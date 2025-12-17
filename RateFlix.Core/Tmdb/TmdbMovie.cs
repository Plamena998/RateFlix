using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Tmdb
{
    public class TmdbMovie
    {
        public string Poster_path { get; set; }
        public int Id { get; set; }
        public string Title { get; set; }
        public string Overview { get; set; }
        public string Release_date { get; set; }
        public decimal Vote_average { get; set; }
        public int[] Genre_ids { get; set; }
    }
}
