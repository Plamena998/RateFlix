using Microsoft.AspNetCore.Mvc;
using RateFlix.Services.Interfaces;

public class MoviesController : Controller
{
    private readonly IMovieService _movieService;

    public MoviesController(IMovieService movieService)
    {
        _movieService = movieService;
    }

    public async Task<IActionResult> Index(string? search, int? genreId, int? year, string? sortBy, int page = 1)
    {
        var model = await _movieService.GetMoviesIndexAsync(search, genreId, year, sortBy, page);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> LoadPage(string? search, int? genreId, int? year, string? sortBy, int page = 1)
    {
        var (movies, currentPage, totalPages, totalMovies) = await _movieService.LoadMoviesPageAsync(search, genreId, year, sortBy, page);
        return Json(new { movies, currentPage, totalPages, totalMovies });
    }

    [HttpGet]
    public async Task<IActionResult> GetContentModal(int id)
    {
        var viewModel = await _movieService.GetMovieWithDetailsAsync(id);
        if (viewModel == null) return NotFound();
        return PartialView("Components/_ContentModal", viewModel);
    }
}
