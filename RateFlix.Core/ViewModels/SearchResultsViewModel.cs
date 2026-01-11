using RateFlix.Core.ViewModels;

namespace RateFlix.Services.Interfaces
{
    public class SearchResultsViewModel
    {
        public List<SearchItemViewModel> Movies { get; set; } = new();

        public List<SearchItemViewModel> Series { get; set; } = new();
    }
}