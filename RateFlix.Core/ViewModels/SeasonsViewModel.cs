using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Core.ViewModels
{
    public class SeasonsViewModel
    {
        public int SeriesId { get; set; }
        public string SeriesTitle { get; set; }
        public List<SeasonViewModel> Seasons { get; set; } = new List<SeasonViewModel>();

    }
}
