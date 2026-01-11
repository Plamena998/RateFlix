using Microsoft.EntityFrameworkCore;
using RateFlix.Core.ViewModels;
using RateFlix.Data;
using RateFlix.Services.Interfaces;
using System;

namespace RateFlix.Services
{
    public class ActorsService : IActorsService
    {
        private readonly AppDbContext _context;
        private const int PageSize = 30;

        public ActorsService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ActorsPageViewModel> GetActorsPageAsync(int page)
        {
            var today = DateTime.Today;

            // Birthday actors
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

            var totalActors = await _context.Actors.CountAsync();
            var totalPages = (int)Math.Ceiling(totalActors / (double)PageSize);

            page = Math.Clamp(page, 1, totalPages);

            var allActors = await _context.Actors
                .OrderBy(a => a.Name)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .Select(a => new ActorCardViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    ImageUrl = a.ImageUrl ?? "/images/actors/default.jpg",
                    BirthDate = a.BirthDate
                })
                .ToListAsync();

            return new ActorsPageViewModel
            {
                BirthdayActors = birthdayActors,
                AllActors = allActors,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalActors = totalActors,
                PageSize = PageSize
            };
        }

        public async Task<ActorModalViewModel?> GetActorModalAsync(int actorId)
        {
            var actor = await _context.Actors
                .Include(a => a.ContentActors)
                    .ThenInclude(ca => ca.Content)
                .FirstOrDefaultAsync(a => a.Id == actorId);

            if (actor == null)
                return null;

            return new ActorModalViewModel
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
                        ContentType = ca.Content is Core.Models.Movie
                            ? "Movies"
                            : "Series"
                    })
                    .ToList()
            };
        }
    }
}
