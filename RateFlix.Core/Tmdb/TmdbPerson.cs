using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.Tmdb
{
    public class TmdbPerson
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Profile_path { get; set; }
        public string? Birthday { get; set; }
        public string Biography { get; set; } = string.Empty;
        public string Known_for_department { get; set; } = string.Empty;
    }
}
