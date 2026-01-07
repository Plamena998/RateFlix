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

        if (data.isFavorite) {
            // Add favorite styling
            heartElement.classList.remove('far');
            heartElement.classList.add('fas', 'text-red-500');
            heartElement.parentElement.classList.add('scale-110');

            // Show notification
            showNotification(data.message, 'success');
        } else {
            // Remove favorite styling
            heartElement.classList.remove('fas', 'text-red-500');
            heartElement.classList.add('far');
            heartElement.parentElement.classList.remove('scale-110');

            // Show notification
            showNotification(data.message, 'info');
        }

        // Animate the heart
        heartElement.parentElement.classList.add('animate-pulse');
        setTimeout(() => {
            heartElement.parentElement.classList.remove('animate-pulse');
        }, 300);

    } catch (error) {
        console.error('Error toggling favorite:', error);
        showNotification('Failed to update favorite', 'error');
    }
}

// Check if content is favorited (call this when opening a modal)
async function checkFavoriteStatus(contentId, heartElement) {
    try {
        const response = await fetch(`/Profile/IsFavorite?contentId=${contentId}`);
        const data = await response.json();

        if (data.isFavorite) {
            heartElement.classList.remove('far');
            heartElement.classList.add('fas', 'text-red-500');
        } else {
            heartElement.classList.remove('fas', 'text-red-500');
            heartElement.classList.add('far');
        }
    } catch (error) {
        console.error('Error checking favorite status:', error);
    }
}

// Show notification toast
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `fixed bottom-4 right-4 px-6 py-3 rounded-lg shadow-lg z-50 transform transition-all duration-300 translate-x-full`;

    // Set color based on type
    if (type === 'success') {
        notification.classList.add('bg-green-500', 'text-white');
    } else if (type === 'error') {
        notification.classList.add('bg-red-500', 'text-white');
    } else {
        notification.classList.add('bg-gray-800', 'text-white');
    }

    notification.innerHTML = `
        <div class="flex items-center gap-2">
            <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-circle' : 'info-circle'}"></i>
            <span class="font-semibold">${message}</span>
        </div>
    `;

    document.body.appendChild(notification);

    // Slide in
    setTimeout(() => {
        notification.classList.remove('translate-x-full');
    }, 10);

    // Slide out and remove
    setTimeout(() => {
        notification.classList.add('translate-x-full');
        setTimeout(() => {
            notification.remove();
        }, 300);
    }, 3000);
}

// Show login modal when user tries to favorite without being logged in
function showLoginModal() {
    const existingModal = document.getElementById('Profile/LoginPrompt');
    if (existingModal) {
        existingModal.remove();
    }

    document.body.appendChild(modal);
    document.body.style.overflow = 'hidden';

    // Close on background click
    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            closeLoginModal();
        }
    });
}

function closeLoginModal() {
    const modal = document.getElementById('loginPrompt');
    if (modal) {
        modal.remove();
        document.body.style.overflow = 'auto';
    }
}