using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RateFlix.Data;
using RateFlix.Models.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RateFlix.Controllers
{
    public class ActorsController : Controller
    {
        private readonly AppDbContext _context;

        public ActorsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            var today = DateTime.Today;
            int pageSize = 30; // 30 actors per page

            // Get birthday actors
            var birthdayActors = await _context.Actors
                .Where(a => a.BirthDate.HasValue &&
                            a.BirthDate.Value.Month == today.Month &&
                            a.BirthDate.Value.Day == today.Day)
                .Include(a => a.ContentActors)
                    .ThenInclude(ca => ca.Content)
                .OrderBy(a => a.Name)
                .Select(a => new ActorCardViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    ImageUrl = a.ImageUrl ?? "/images/actors/default.jpg",
                    BirthDate = a.BirthDate,
                    TopContents = a.ContentActors
                        .Select(ca => ca.Content)
                        .OrderByDescending(c => c.IMDBScore)
                        .Take(3)
                        .ToList()
                })
                .ToListAsync();

            // Get total count for pagination
            var totalActors = await _context.Actors.CountAsync();
            var totalPages = (int)Math.Ceiling(totalActors / (double)pageSize);

            // Ensure page is within valid range
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            // Get paginated actors
            var allActors = await _context.Actors
                .OrderBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ActorCardViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    ImageUrl = a.ImageUrl ?? "/images/actors/default.jpg",
                    BirthDate = a.BirthDate
                })
                .ToListAsync();

            var model = new ActorsPageViewModel
            {
                BirthdayActors = birthdayActors,
                AllActors = allActors,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalActors = totalActors,
                PageSize = pageSize
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetActorModal(int actorId)
        {
            var actor = await _context.Actors
                .Include(a => a.ContentActors)
                    .ThenInclude(ca => ca.Content)
                .FirstOrDefaultAsync(a => a.Id == actorId);

            if (actor == null)
                return NotFound();

            var modalVm = new ActorModalViewModel
            {
                Id = actor.Id,
                Name = actor.Name,
                ImageUrl = actor.ImageUrl ?? "/images/actors/default.jpg",
                BirthDate = actor.BirthDate,
                TopRoles = actor.ContentActors
                    .OrderByDescending(ca => ca.Content.IMDBScore)
                    .Take(6)
                    .Select(ca => new ContentRoleViewModel
                    {
                        ContentId = ca.Content.Id,
                        Title = ca.Content.Title,
                        ImageUrl = ca.Content.ImageUrl,
                        IMDBScore = ca.Content.IMDBScore,
                        ReleaseYear = ca.Content.ReleaseYear,
                        ContentType = ca.Content is RateFlix.Data.Models.Movie ? "Movies" : "Series"
                    })
                    .ToList()
            };

            return PartialView("_ActorModal", modalVm);
        }
    }
}