
namespace RateFlix.Services.Interfaces
{
    public interface IReviewService
    {
        Task<(bool Success, string Message, double? NewRating)> SubmitReviewAsync(
            string userId,
            int contentId,
            int score,
            string? review);
    }
}
