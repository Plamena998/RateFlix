
using RateFlix.Core.ViewModels;

namespace RateFlix.Services.Interfaces
{
    public interface IActorsService
    {
        Task<ActorsPageViewModel> GetActorsPageAsync(int page);
        Task<ActorModalViewModel?> GetActorModalAsync(int actorId);
    }
}
