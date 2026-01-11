using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RateFlix.Services;

namespace RateFlix.Controllers
{
    [Authorize(Roles = "Administrator")]
    public class AdminAnalyticsController : Controller
    {
        private readonly IAnalyticsService _analyticsService;

        public AdminAnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        public async Task<IActionResult> Index()
        {
            var model = await _analyticsService.GetAdminAnalyticsAsync();
            return View(model);
        }
    }
}
