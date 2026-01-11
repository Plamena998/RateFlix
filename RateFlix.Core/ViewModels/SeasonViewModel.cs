using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.ViewModels
{
    public class SeasonViewModel
    {
        public int Id { get; set; }
        public int SeasonNumber { get; set; }
        public double IMDBScore { get; set; }
        public List<EpisodeViewModel> Episodes { get; set; } = new List<EpisodeViewModel>();

    }
}
