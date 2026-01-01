// ================================
// Universal Content Modal JS
// ================================

// Track selected ratings per contentId
const selectedRatings = {};

// Load Content Modal (Movies/Series/Episodes)
function loadContentModal(contentId, contentType = 'Movies') {
    console.log('Loading modal:', contentId, contentType);

    $.ajax({
        url: `/${contentType}/GetContentModal/${contentId}`,
        type: 'GET',
        success: function (html) {
            $('#contentModalContainer').html(html);
            openContentModal(contentId);
        },
        error: function (xhr, status, error) {
            console.error('Error loading modal:', xhr.status, error);
            alert('Failed to load details');
        }
    });
}

// Open Modal
function openContentModal(contentId) {
    const modal = document.getElementById(`contentModal-${contentId}`);
    if (modal) {
        modal.classList.remove('hidden');
        document.body.style.overflow = 'hidden';
    }
}

// Close Modal
function closeContentModal(contentId) {
    const modal = document.getElementById(`contentModal-${contentId}`);
    if (modal) {
        modal.classList.add('hidden');
        document.body.style.overflow = 'auto';
        setTimeout(() => {
            $('#contentModalContainer').html('');
        }, 300);
    }
}

// ================================
// STAR RATING FUNCTIONS
// ================================

// Highlight stars on hover
function highlightStars(contentId, index) {
    const container = document.getElementById(`starRating-${contentId}`);
    if (!container) return;
    const stars = container.querySelectorAll('.star-btn');
    stars.forEach((star, i) => {
        if (i < index) {
            star.classList.add('text-yellow-500');
            star.classList.remove('text-gray-600');
        } else {
            star.classList.remove('text-yellow-500');
            star.classList.add('text-gray-600');
        }
    });
}

// Reset stars to selected rating
function resetStars(contentId) {
    const rating = selectedRatings[contentId] || 0;
    highlightStars(contentId, rating);
}

// Rate content
function rateContent(contentId, rating) {
    selectedRatings[contentId] = rating;
    highlightStars(contentId, rating);

    const ratingDisplay = document.getElementById(`selectedRating-${contentId}`);
    if (ratingDisplay) {
        ratingDisplay.textContent = `You rated ${rating}/10`;
        ratingDisplay.dataset.rating = rating;
        ratingDisplay.classList.remove('hidden');
    }
}

// ================================
// SUBMIT REVIEW + RATING
// ================================

function submitReview(contentId, contentType) {
    const rating = selectedRatings[contentId];
    if (!rating) {
        alert('Please select a rating (click on the stars)');
        return;
    }

    const reviewText = document.getElementById(`reviewText-${contentId}`).value || '';
    const token = $('input[name="__RequestVerificationToken"]').val();

    const submitBtn = event ? event.target.closest('button') : null;
    const originalText = submitBtn ? submitBtn.innerHTML : '';

    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i>Submitting...';
    }

    $.ajax({
        url: '/Movies/SubmitRating', // adjust if using Reviews/Create
        type: 'POST',
        headers: token ? { 'RequestVerificationToken': token } : {},
        data: {
            movieId: contentId,
            score: rating,
            review: reviewText
        },
        success: function (response) {
            if (response.success) {
                updateContentRating(contentId, response.newRating);

                if (submitBtn) {
                    submitBtn.innerHTML = '<i class="fas fa-check mr-2"></i>Review Submitted!';
                    submitBtn.classList.remove('bg-red-500', 'hover:bg-red-600');
                    submitBtn.classList.add('bg-green-500');
                }

                setTimeout(() => {
                    closeContentModal(contentId);
                    selectedRatings[contentId] = 0;

                    // Reload page if on details page
                    if (window.location.pathname.includes('/Details/')) {
                        location.reload();
                    }
                }, 1500);
            } else {
                alert('Failed to submit review: ' + (response.message || 'Unknown error'));
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }
            }
        },
        error: function (xhr) {
            console.error('Review submission error:', xhr.responseText);
            alert('Error submitting review. Please try again.');
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        }
    });
}

// Update displayed rating (modal + grid)
function updateContentRating(contentId, newRating) {
    // Update modal
    const modalBadge = document.querySelector(`#contentModal-${contentId} .bg-yellow-500`);
    if (modalBadge) modalBadge.innerHTML = `<i class="fas fa-star"></i> ${newRating}`;

    // Update grid card
    const gridCard = document.querySelector(`.movie-card[data-movie-id='${contentId}'] .imdb-score`);
    if (gridCard) gridCard.innerHTML = `<i class="fas fa-star"></i> ${newRating}`;
}

// ================================
// FAVORITES
// ================================

function toggleFavorite(contentId) {
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Favorites/Toggle',
        type: 'POST',
        headers: token ? { 'RequestVerificationToken': token } : {},
        data: { contentId: contentId },
        success: function (response) {
            if (response.success) {
                const btn = event.target.closest('button');
                const icon = btn.querySelector('i');

                if (response.isFavorite) {
                    icon.classList.remove('far');
                    icon.classList.add('fas');
                    alert('Added to favorites!');
                } else {
                    icon.classList.remove('fas');
                    icon.classList.add('far');
                    alert('Removed from favorites!');
                }
            }
        },
        error: function () {
            alert('Error updating favorites.');
        }
    });
}

// Close modal on ESC
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        document.querySelectorAll('[id^="contentModal-"]').forEach(modal => {
            const modalId = modal.id.replace('contentModal-', '');
            closeContentModal(modalId);
        });
        document.body.style.overflow = 'auto';
    }
});
// ========== SEASONS MODAL FUNCTIONS ==========

// Load Seasons Modal
function loadSeasonsModal(seriesId) {
    console.log('Loading seasons modal for series:', seriesId);

    // Close content modal first
    closeContentModal(seriesId);

    $.ajax({
        url: `/Series/GetSeasonsModal/${seriesId}`,
        type: 'GET',
        success: function (html) {
            $('#seasonsModalContainer').html(html);
            openSeasonsModal(seriesId);
        },
        error: function (xhr, status, error) {
            console.error('Error loading seasons modal:', xhr.status, error);
            alert('Failed to load seasons');
        }
    });
}

// Open Seasons Modal
function openSeasonsModal(seriesId) {
    const modal = document.getElementById(`seasonsModal-${seriesId}`);
    if (modal) {
        modal.classList.remove('hidden');
        document.body.style.overflow = 'hidden';
    }
}

// Close Seasons Modal
function closeSeasonsModal(seriesId) {
    const modal = document.getElementById(`seasonsModal-${seriesId}`);
    if (modal) {
        modal.classList.add('hidden');
        document.body.style.overflow = 'auto';
        setTimeout(() => {
            $('#seasonsModalContainer').html('');
        }, 300);
    }
}

// Toggle Season Collapse
function toggleSeason(seasonId) {
    const seasonDiv = document.getElementById(`season-${seasonId}`);
    const chevron = document.getElementById(`chevron-${seasonId}`);

    if (seasonDiv && chevron) {
        seasonDiv.classList.toggle('hidden');
        chevron.classList.toggle('rotate-180');
    }
}

// ========== EPISODE RATING FUNCTIONS ==========

// Rate Episode
function rateEpisode(episodeId, rating, event) {
    event.stopPropagation(); // Prevent parent clicks

    const stars = document.querySelectorAll(`#episodeStars-${episodeId} .episode-star-btn`);

    // Visual feedback
    stars.forEach((star, index) => {
        if (index < rating) {
            star.classList.remove('text-gray-600');
            star.classList.add('text-yellow-400');
        } else {
            star.classList.remove('text-yellow-400');
            star.classList.add('text-gray-600');
        }
    });

    // Submit rating to server
    submitEpisodeRating(episodeId, rating);
}

// Submit Episode Rating
function submitEpisodeRating(episodeId, rating) {
    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Reviews/CreateEpisodeReview',
        type: 'POST',
        headers: token ? { 'RequestVerificationToken': token } : {},
        data: {
            episodeId: episodeId,
            rating: rating
        },
        success: function (response) {
            if (response.success) {
                // Show success feedback
                const container = document.querySelector(`#episodeStars-${episodeId}`).parentElement;
                const feedback = document.createElement('p');
                feedback.className = 'text-xs text-green-500 mt-1 text-center';
                feedback.textContent = 'Rating saved!';
                container.appendChild(feedback);

                // Remove feedback after 2 seconds
                setTimeout(() => {
                    feedback.remove();
                }, 2000);

                // Update episode rating display if returned
                if (response.newAverageRating) {
                    updateEpisodeRating(episodeId, response.newAverageRating);
                }
            } else {
                alert('Failed to save rating: ' + (response.message || 'Unknown error'));
            }
        },
        error: function (xhr) {
            console.error('Episode rating error:', xhr.responseText);
            alert('Error submitting rating. Please try again.');
        }
    });
}

// Update Episode Rating Display
function updateEpisodeRating(episodeId, newRating) {
    const container = document.querySelector(`#episodeStars-${episodeId}`).closest('.bg-gray-800');
    const ratingBadge = container.querySelector('.bg-yellow-500');

    if (ratingBadge) {
        ratingBadge.innerHTML = `<i class="fas fa-star"></i> ${newRating}`;
    }
}

// Update ESC key handler to include seasons modal
document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape') {
        document.querySelectorAll('[id^="contentModal-"]').forEach(modal => {
            const modalId = modal.id.replace('contentModal-', '');
            closeContentModal(modalId);
        });
        document.querySelectorAll('[id^="seasonsModal-"]').forEach(modal => {
            const modalId = modal.id.replace('seasonsModal-', '');
            closeSeasonsModal(modalId);
        });
        document.body.style.overflow = 'auto';
    }
});