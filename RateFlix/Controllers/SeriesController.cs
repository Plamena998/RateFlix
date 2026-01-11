using Microsoft.AspNetCore.Mvc;
using RateFlix.Services.Interfaces;

public class SeriesController : Controller
{
    private readonly ISeriesService _seriesService;

    public SeriesController(ISeriesService seriesService)
    {
        _seriesService = seriesService;
    }

    public async Task<IActionResult> Index(string? search, int? genreId, int? year, string? sortBy, int page = 1)
    {
        var model = await _seriesService.GetSeriesIndexAsync(search, genreId, year, sortBy, page);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> LoadPage(string? search, int? genreId, int? year, string? sortBy, int page = 1)
    {
        var (series, currentPage, totalPages, totalSeries) = await _seriesService.LoadSeriesPageAsync(search, genreId, year, sortBy, page);
        return Json(new { series, currentPage, totalPages, totalSeries });
    }

    [HttpGet]
    public async Task<IActionResult> GetContentModal(int id)
    {
        var series = await _seriesService.GetSeriesWithDetailsAsync(id);
        if (series == null) return NotFound();
        return PartialView("Components/_ContentModal", series);
    }

    [HttpGet]
    public async Task<IActionResult> GetSeasonsModal(int id)
    {
        var series = await _seriesService.GetSeriesWithSeasonsAsync(id);
        if (series == null) return NotFound();
        return PartialView("Components/_SeasonsModal", series);
    }
}
