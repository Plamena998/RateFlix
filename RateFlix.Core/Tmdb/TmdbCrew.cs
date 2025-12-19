using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Tmdb
{
    public class TmdbCrew
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Profile_path { get; set; }
        public string Job { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }
}
