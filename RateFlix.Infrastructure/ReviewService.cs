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
                // Check if user already reviewed this content
                var existingReview = await _context.Reviews
                    .FirstOrDefaultAsync(r => r.ContentId == contentId && r.UserId == userId);

                if (existingReview != null)
                {
                    // Update existing review
                    if (score > 0) existingReview.Rating = score;
                    if (!string.IsNullOrWhiteSpace(review)) existingReview.Comment = review;
                    existingReview.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Create new review
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
                    .AverageAsync(r => (double)r.Rating);

                avgRating = Math.Round(avgRating, 1);

                await UpdateContentRatingAsync(contentId, avgRating);

                return (true, "Review saved successfully.", avgRating);
            }
            catch (Exception ex)
            {
                return (false, "Failed to save review: " + ex.Message, null);
            }
        }

        private async Task UpdateContentRatingAsync(int contentId, double newRating)
        {
            var content = await _context.Set<Content>().FindAsync(contentId);
            if (content != null)
            {
                content.IMDBScore = newRating;
                await _context.SaveChangesAsync();
            }
        }
        public async Task<(bool Success, string Message)> DeleteReviewAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null)
                    return (false, "Review not found.");

                var contentId = review.ContentId;

                // Delete the review
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                // Recalculate average rating after deletion
                var remainingReviews = await _context.Reviews
                    .Where(r => r.ContentId == contentId)
                    .ToListAsync();

                if (remainingReviews.Any())
                {
                    // If there are still reviews, calculate new average
                    var avgRating = remainingReviews.Average(r => (double)r.Rating);
                    avgRating = Math.Round(avgRating, 1);
                    await UpdateContentRatingAsync(contentId, avgRating);
                }
                else
                {
                    // If no reviews left, set score back to 0
                    await UpdateContentRatingAsync(contentId, 0);
                }

                return (true, "Review deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, "Failed to delete review: " + ex.Message);
            }
        }
    }
}