using Microsoft.EntityFrameworkCore;
using RateFlix.Core.Models;
using RateFlix.Data;
using RateFlix.Services.Interfaces;

namespace RateFlix.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDbContext _context;

        public ReviewService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<(bool Success, string Message, double? NewRating)> SubmitReviewAsync(
            string userId,
            int contentId,
            int score,
            string? review)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return (false, "User not authenticated.", null);

            if (score <= 0 && string.IsNullOrWhiteSpace(review))
                return (false, "Please provide a rating or a comment.", null);

            try
            {
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ContentId == contentId && r.UserId == userId);

                if (existingReview != null)
                {
                    if (score > 0) existingReview.Rating = score;
                    if (!string.IsNullOrWhiteSpace(review)) existingReview.Comment = review;
                    existingReview.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newReview = new Review
                    {
                        ContentId = contentId,
                        UserId = userId,
                        Rating = score,
                        Comment = review ?? string.Empty,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Reviews.Add(newReview);
                }

                await _context.SaveChangesAsync();

                var avgRating = await _context.Reviews
                    .Where(r => r.ContentId == contentId)
                    .AverageAsync(r => r.Rating);

                return (true, "Review saved successfully.", avgRating);
            }
            catch (Exception ex)
            {
                return (false, "Failed to save review: " + ex.Message, null);
            }
        }
    }
}
