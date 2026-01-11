using Microsoft.AspNetCore.Mvc;
using RateFlix.Services.Interfaces;

namespace RateFlix.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeService _homeService;

        public HomeController(IHomeService homeService)
        {
            _homeService = homeService;
        }

        public async Task<IActionResult> Index()
            => View(await _homeService.GetHomeAsync());

        [HttpGet]
        public async Task<IActionResult> Search(string query)
            => Json(await _homeService.SearchAsync(query));

        [HttpGet]
        public async Task<IActionResult> LoadMoreMovies(int skip = 0, int take = 10)
            => Json(await _homeService.LoadMoreMoviesAsync(skip, take));

        public async Task<IActionResult> NewsSection()
            => PartialView("_NewsSection", await _homeService.GetNewsSectionAsync());
    }
}
