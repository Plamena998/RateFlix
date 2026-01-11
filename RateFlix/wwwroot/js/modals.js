
const selectedRatings = {};

// Load Content Modal (Movies / Series / Episodes)
function loadContentModal(contentId, contentType = 'Movies') {
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

function openContentModal(contentId) {
    const modal = document.getElementById(`contentModal-${contentId}`);
    if (modal) {
        modal.classList.remove('hidden');
        document.body.style.overflow = 'hidden';
    }
}

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

// STAR RATING

function highlightStars(contentId, index) {
    const container = document.getElementById(`starRating-${contentId}`);
    if (!container) return;
    const stars = container.querySelectorAll('.star-btn');
    stars.forEach((star, i) => {
        star.classList.toggle('text-yellow-500', i < index);
        star.classList.toggle('text-gray-500', i >= index);
    });
}

function resetStars(contentId) {
    highlightStars(contentId, selectedRatings[contentId] || 0);
}

function rateContent(contentId, rating) {
    selectedRatings[contentId] = rating;
    highlightStars(contentId, rating);
}

// Series stars
function rateSeries(seriesId, rating) {
    selectedRatings[seriesId] = rating;
    const stars = document.querySelectorAll(`#seriesStars-${seriesId} i`);
    stars.forEach((star, index) => {
        star.classList.toggle('text-yellow-400', index < rating);
        star.classList.toggle('text-gray-500', index >= rating);
    });
}

// SUBMIT REVIEW (Movies & Series unified)

function submitReviewUnified(contentId, isSeries = false) {
    const rating = selectedRatings[contentId] || 0;
    const textareaId = isSeries ? `seriesReviewText-${contentId}` : `reviewText-${contentId}`;
    const reviewTextEl = document.getElementById(textareaId);
    const review = reviewTextEl ? reviewTextEl.value.trim() : '';
    const submitBtn = reviewTextEl ? reviewTextEl.closest('div').querySelector('button') : null;
    const originalText = submitBtn ? submitBtn.innerHTML : '';

    if (!rating && review === '') {
        alert('Please provide a rating or a comment.');
        return;
    }

    if (submitBtn) {
        submitBtn.disabled = true;
        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin mr-2"></i> Sending...';
    }

    const token = $('input[name="__RequestVerificationToken"]').val();

    $.ajax({
        url: '/Reviews/SubmitReview',
        type: 'POST',
        headers: token ? { 'RequestVerificationToken': token } : {},
        data: {
            contentId: contentId,
            score: rating,
            review: review
        },
        success: function (res) {
            if (!res.success) {
                alert('Failed to save review: ' + (res.message || 'Unknown error'));
                if (submitBtn) {
                    submitBtn.disabled = false;
                    submitBtn.innerHTML = originalText;
                }
                return;
            }

            const badge = document.querySelector(`#contentModal-${contentId} .bg-yellow-500`);
            if (badge) badge.innerHTML = `<i class="fas fa-star"></i> ${res.newRating.toFixed(1)}`;
            const gridCard = document.querySelector(`.movie-card[data-movie-id='${contentId}'] .imdb-score`);
            if (gridCard) gridCard.innerHTML = `<i class="fas fa-star"></i> ${res.newRating.toFixed(1)}`;

            if (submitBtn) {
                submitBtn.innerHTML = '<i class="fas fa-check mr-2"></i> Send';
                submitBtn.classList.remove('bg-red-500', 'hover:bg-red-600');
                submitBtn.classList.add('bg-green-500');
            }
        },
        error: function (xhr) {
            console.error('Error submitting review:', xhr.responseText);
            alert('Error submitting review');
            if (submitBtn) {
                submitBtn.disabled = false;
                submitBtn.innerHTML = originalText;
            }
        }
    });
}
// SERIES MODALS

function loadSeasonsModal(seriesId) {
    closeContentModal(seriesId);
    $.ajax({
        url: `/Series/GetSeasonsModal/${seriesId}`,
        type: 'GET',
        success: function (html) {
            $('#seasonsModalContainer').html(html);
            openSeasonsModal(seriesId);
        },
        error: function (xhr) {
            console.error('Error loading seasons modal:', xhr.responseText);
            alert('Failed to load seasons');
        }
    });
}

function openSeasonsModal(seriesId) {
    const modal = document.getElementById(`seasonsModal-${seriesId}`);
    if (modal) {
        modal.classList.remove('hidden');
        document.body.style.overflow = 'hidden';
    }
}

function closeSeasonsModal(seriesId) {
    const modal = document.getElementById(`seasonsModal-${seriesId}`);
    if (modal) {
        modal.classList.add('hidden');
        document.body.style.overflow = 'auto';
        setTimeout(() => $('#seasonsModalContainer').html(''), 300);
    }
}

function toggleSeason(seasonId) {
    const seasonDiv = document.getElementById(`season-${seasonId}`);
    const chevron = document.getElementById(`chevron-${seasonId}`);
    if (seasonDiv && chevron) {
        seasonDiv.classList.toggle('hidden');
        chevron.classList.toggle('rotate-180');
    }
}

// FAVORITES

function toggleFavorite(contentId) {
    const token = $('input[name="__RequestVerificationToken"]').val();
    $.ajax({
        url: '/Favorites/Toggle',
        type: 'POST',
        headers: token ? { 'RequestVerificationToken': token } : {},
        data: { contentId },
        success: function (res) {
            if (res.success) {
                const btn = event.target.closest('button');
                const icon = btn.querySelector('i');
                if (res.isFavorite) icon.classList.replace('far', 'fas');
                else icon.classList.replace('fas', 'far');
            }
        },
        error: function () { alert('Error updating favorites'); }
    });
}

