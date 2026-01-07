// Load and open actor modal
function loadActorModal(actorId) {
    console.log('Loading actor modal for ID:', actorId);

    fetch(`/Actors/GetActorModal?actorId=${actorId}`)
        .then(res => {
            if (!res.ok) {
                throw new Error(`HTTP error! status: ${res.status}`);
            }
            return res.text();
        })
        .then(html => {
            const container = document.getElementById("actorModalContainer");
            if (!container) {
                console.error('actorModalContainer not found in DOM');
                alert('Modal container not found. Please refresh the page.');
                return;
            }

            container.innerHTML = html;

            requestAnimationFrame(() => {
                const modal = document.getElementById(`actorModal-${actorId}`);
                if (!modal) {
                    console.error(`Modal actorModal-${actorId} not found after insertion`);
                    console.log('Container HTML:', container.innerHTML.substring(0, 200));
                    alert('Modal element not found. Check console for details.');
                    return;
                }

                modal.classList.remove('hidden');
                document.body.style.overflow = 'hidden';

                requestAnimationFrame(() => {
                    const content = modal.querySelector('.bg-gradient-to-br');
                    if (content) {
                        content.classList.remove('scale-95');
                        content.classList.add('scale-100');
                    }
                });
            });
        })
        .catch(err => {
            console.error('Failed to load actor modal:', err);
            alert('Failed to load actor details: ' + err.message);
        });
}

function closeActorModal() {
    const openModal = document.querySelector('[id^="actorModal-"]:not(.hidden)');
    if (!openModal) {
        console.log('No open modal found');
        return;
    }

    const content = openModal.querySelector('.bg-gradient-to-br');
    if (content) {
        content.classList.remove('scale-100');
        content.classList.add('scale-95');
    }

    setTimeout(() => {
        openModal.classList.add('hidden');
        document.body.style.overflow = 'auto';

        const container = document.getElementById('actorModalContainer');
        if (container) {
            setTimeout(() => {
                container.innerHTML = '';
            }, 300);
        }
    }, 300);
}

function openContentModalAndCloseActor(contentId, contentType) {
    closeActorModal();

    setTimeout(() => {
        if (typeof loadContentModal === 'function') {
            loadContentModal(contentId, contentType);
        } else if (typeof openContentModal === 'function') {
            openContentModal(contentId, contentType);
        } else {
            console.log('No content modal function found, redirecting...');
            window.location.href = `/${contentType}/Details/${contentId}`;
        }
    }, 350);
}

document.addEventListener('click', function (event) {
    if (event.target.id && event.target.id.startsWith('actorModal-')) {
        closeActorModal();
    }
});

document.addEventListener('keydown', function (event) {
    if (event.key === 'Escape') {
        closeActorModal();
    }
});