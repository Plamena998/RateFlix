// Toggle favorite status
async function toggleFavorite(contentId, heartElement) {
    try {
        const response = await fetch('/Profile/ToggleFavorite', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ contentId: contentId })
        });

        const data = await response.json();

        // Check if user needs to log in
        if (data.message === "Please log in to add favorites") {
            showLoginModal();
            return;
        }

        // Update heart icon based on favorite status
        updateHeartIcon(heartElement, data.isFavorite);

        // Show success notification
        showNotification(data.message, data.isFavorite ? 'success' : 'info');

    } catch (error) {
        console.error('Error toggling favorite:', error);
        showNotification('Failed to update favorite', 'error');
    }
}

// Update heart icon appearance
function updateHeartIcon(heartElement, isFavorite) {
    if (isFavorite) {
        // Add favorite styling
        heartElement.classList.remove('far');
        heartElement.classList.add('fas', 'text-red-500');
        heartElement.parentElement.classList.add('scale-110');
    } else {
        // Remove favorite styling
        heartElement.classList.remove('fas', 'text-red-500');
        heartElement.classList.add('far');
        heartElement.parentElement.classList.remove('scale-110');
    }

    // Pulse animation
    heartElement.parentElement.classList.add('animate-pulse');
    setTimeout(() => {
        heartElement.parentElement.classList.remove('animate-pulse');
    }, 300);
}

// Check if content is favorited (call this when opening a modal)
async function checkFavoriteStatus(contentId, heartElement) {
    try {
        const response = await fetch(`/Profile/IsFavorite?contentId=${contentId}`);
        const data = await response.json();

        updateHeartIcon(heartElement, data.isFavorite);
    } catch (error) {
        console.error('Error checking favorite status:', error);
    }
}

// Show notification toast
function showNotification(message, type = 'info') {
    // Remove any existing notifications
    const existingNotification = document.querySelector('.notification-toast');
    if (existingNotification) {
        existingNotification.remove();
    }

    const notification = document.createElement('div');
    notification.className = 'notification-toast fixed bottom-4 right-4 px-6 py-3 rounded-lg shadow-lg z-50 transform transition-all duration-300 translate-x-full';

    // Set color and icon based on type
    const config = {
        success: { bg: 'bg-green-500', icon: 'check-circle' },
        error: { bg: 'bg-red-500', icon: 'exclamation-circle' },
        info: { bg: 'bg-gray-800', icon: 'info-circle' }
    };

    const { bg, icon } = config[type] || config.info;
    notification.classList.add(bg, 'text-white');

    notification.innerHTML = `
        <div class="flex items-center gap-2">
            <i class="fas fa-${icon}"></i>
            <span class="font-semibold">${message}</span>
        </div>
    `;

    document.body.appendChild(notification);

    // Slide in
    requestAnimationFrame(() => {
        notification.classList.remove('translate-x-full');
    });

    // Slide out and remove after 3 seconds
    setTimeout(() => {
        notification.classList.add('translate-x-full');
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

// Show login modal when user tries to favorite without being logged in
async function showLoginModal() {
    // Check if modal already exists
    const existingModal = document.getElementById('loginPromptModal');
    if (existingModal) {
        existingModal.style.display = 'block';
        document.body.style.overflow = 'hidden';
        return;
    }

    try {
        // Fetch the login prompt view from server
        const response = await fetch('/Profile/LoginPrompt');

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const html = await response.text();

        // Create container and insert the HTML
        const modalContainer = document.createElement('div');
        modalContainer.id = 'loginPromptModal';
        modalContainer.innerHTML = html;

        document.body.appendChild(modalContainer);
        document.body.style.overflow = 'hidden';

        // Close on background click
        const modalOverlay = modalContainer.querySelector('#loginPromptOverlay');
        if (modalOverlay) {
            modalOverlay.addEventListener('click', (e) => {
                if (e.target === modalOverlay) {
                    closeLoginModal();
                }
            });
        }

        // Close on Escape key
        const handleEscape = (e) => {
            if (e.key === 'Escape') {
                closeLoginModal();
                document.removeEventListener('keydown', handleEscape);
            }
        };
        document.addEventListener('keydown', handleEscape);

    } catch (error) {
        console.error('Error loading login modal:', error);
        showNotification('Please log in to add favorites', 'info');

        // Fallback: redirect to login page after a short delay
        setTimeout(() => {
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
        }, 1500);
    }
}

// Close login modal
function closeLoginModal() {
    const modal = document.getElementById('loginPromptModal');
    if (modal) {
        // Fade out animation
        modal.style.opacity = '0';
        modal.style.transition = 'opacity 0.3s ease-out';

        setTimeout(() => {
            modal.remove();
            document.body.style.overflow = 'auto';
        }, 300);
    }
}

// Initialize favorites on page load
document.addEventListener('DOMContentLoaded', () => {
    // Check favorite status for any visible heart icons
    const heartIcons = document.querySelectorAll('[id^="favoriteHeart-"]');
    heartIcons.forEach(heart => {
        const contentId = heart.id.split('-')[1];
        if (contentId) {
            checkFavoriteStatus(parseInt(contentId), heart);
        }
    });
});