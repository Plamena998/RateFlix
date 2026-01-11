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

                var newRating = await CalculateWeightedRatingAsync(contentId);

                return (true, "Review saved successfully.", newRating);
            }
            catch (Exception ex)
            {
                return (false, "Failed to save review: " + ex.Message, null);
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

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();

                await CalculateWeightedRatingAsync(contentId);

                return (true, "Review deleted successfully.");
            }
            catch (Exception ex)
            {
                return (false, "Failed to delete review: " + ex.Message);
            }
        }

        private async Task<double> CalculateWeightedRatingAsync(int contentId)
        {
            var content = await _context.Set<Content>().FindAsync(contentId);
            if (content == null)
                return 0;

            var allReviewsEver = await _context.Reviews
                .Where(r => r.ContentId == contentId)
                .ToListAsync();

            var reviewCount = allReviewsEver.Count;

            if (reviewCount == 0)
            {
                return content.IMDBScore;
            }

            var userAverage = allReviewsEver.Average(r => (double)r.Rating);


            const int imdbWeight = 10; // Treat IMDB score as 10 votes

            // avrg: (imdbScore * weight + sum of user ratings) / (weight + number of reviews)
            var weightedScore = (content.IMDBScore * imdbWeight + allReviewsEver.Sum(r => r.Rating))
                                / (imdbWeight + reviewCount);

            var finalScore = Math.Round(weightedScore, 1);

            content.IMDBScore = finalScore;
            await _context.SaveChangesAsync();

            return finalScore;
        }
    }
}