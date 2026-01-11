using Microsoft.AspNetCore.Mvc;
using RateFlix.Services.Interfaces;

namespace RateFlix.Controllers
{
    public class ActorsController : Controller
    {
        private readonly IActorsService _actorsService;

        public ActorsController(IActorsService actorsService)
        {
            _actorsService = actorsService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var model = await _actorsService.GetActorsPageAsync(page);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetActorModal(int actorId)
        {
            var modalVm = await _actorsService.GetActorModalAsync(actorId);

            if (modalVm == null)
                return NotFound();

            return PartialView("_ActorModal", modalVm);
        }
    }
}
