using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Tmdb
{
    public class TmdbCreditsResponse
    {
        public int Id { get; set; }
        public List<TmdbCast> Cast { get; set; } = new();
        public List<TmdbCrew> Crew { get; set; } = new();
    }
}
