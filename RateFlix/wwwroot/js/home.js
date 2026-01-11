$(document).ready(function () {
    let skip = 10;

    // Search functionality
    let searchTimeout;
    $('#searchInput').on('input', function () {
        clearTimeout(searchTimeout);
        const query = $(this).val().trim();

        if (query.length < 2) {
            $('#searchResults').addClass('hidden');
            return;
        }

        searchTimeout = setTimeout(function () {
            $.ajax({
                url: '/Home/Search',
                type: 'GET',
                data: { query: query },
                success: function (data) {
                    displaySearchResults(data);
                },
                error: function () {
                    console.error('Search failed');
                }
            });
        }, 300);
    });

    function displaySearchResults(data) {
        const resultsDiv = $('#searchResults');
        resultsDiv.empty();

        if (data.movies.length === 0 && data.series.length === 0) {
            resultsDiv.html('<div class="p-4 text-gray-400">No results found</div>');
            resultsDiv.removeClass('hidden');
            return;
        }

        let html = '';

        if (data.movies.length > 0) {
            html += '<div class="p-4"><h3 class="font-bold text-red-500 mb-2">Movies</h3>';
            data.movies.forEach(movie => {
                html += `
                <div class="search-result-item flex items-center gap-4 p-2 hover:bg-gray-700 rounded transition cursor-pointer" 
                     data-id="${movie.id}" 
                     data-type="Movies">
                    <img src="${movie.imageUrl}" alt="${movie.title}" class="w-12 h-16 object-cover rounded" onerror="this.src='https://via.placeholder.com/500x750?text=No+Image'" />
                    <div>
                        <div class="font-semibold">${movie.title}</div>
                        <div class="text-sm text-gray-400">${movie.releaseYear} • <i class="fas fa-star text-yellow-500"></i> ${movie.imdbScore}</div>
                    </div>
                </div>
            `;
            });
            html += '</div>';
        }

        if (data.series.length > 0) {
            html += '<div class="p-4"><h3 class="font-bold text-red-500 mb-2">Series</h3>';
            data.series.forEach(series => {
                html += `
                <div class="search-result-item flex items-center gap-4 p-2 hover:bg-gray-700 rounded transition cursor-pointer" 
                     data-id="${series.id}" 
                     data-type="Series">
                    <img src="${series.imageUrl}" alt="${series.title}" class="w-12 h-16 object-cover rounded" onerror="this.src='https://via.placeholder.com/500x750?text=No+Image'" />
                    <div>
                        <div class="font-semibold">${series.title}</div>
                        <div class="text-sm text-gray-400">${series.releaseYear} • <i class="fas fa-star text-yellow-500"></i> ${series.imdbScore}</div>
                    </div>
                </div>
            `;
            });
            html += '</div>';
        }

        resultsDiv.html(html);
        resultsDiv.removeClass('hidden');

        // Add click handlers to search results (remove old handlers first)
        $('.search-result-item').off('click').on('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            const id = $(this).data('id');
            const type = $(this).data('type');

 

            // Hide search results
            $('#searchResults').addClass('hidden');

            // Clear search input (optional)
            $('#searchInput').val('');

            // Load content modal
            loadContentModal(id, type);
        });
    }

    // Hide search results when clicking outside
    $(document).on('click', function (e) {
        if (!$(e.target).closest('#searchInput, #searchResults').length) {
            $('#searchResults').addClass('hidden');
        }
    });

    // Load More functionality
    $('#loadMoreBtn').on('click', function () {
        const button = $(this);
        button.prop('disabled', true).text('Loading...');

        $.ajax({
            url: '/Home/LoadMoreMovies',
            type: 'GET',
            data: { skip: skip, take: 10 },
            success: function (movies) {
                if (movies.length === 0) {
                    button.text('No more movies');
                    return;
                }

                movies.forEach(movie => {
                    let genresHtml = '';
                    if (movie.genres && movie.genres.length > 0) {
                        genresHtml = '<div class="flex flex-wrap gap-1 mt-2">';
                        movie.genres.forEach(genre => {
                            genresHtml += `<span class="bg-red-500 px-2 py-0.5 rounded text-xs text-white">${genre}</span>`;
                        });
                        genresHtml += '</div>';
                    }

                    const card = `
                <div class="movie-card group cursor-pointer transform transition duration-300 hover:scale-105" onclick="loadContentModal(${movie.id}, 'Movies')">
                    <div class="relative overflow-hidden rounded-lg shadow-lg">
                        <img src="${movie.imageUrl}" 
                             alt="${movie.title}" 
                             class="w-full h-80 object-cover group-hover:opacity-75 transition"
                             onerror="this.src='https://via.placeholder.com/500x750?text=No+Image'" />
                        <div class="absolute top-2 right-2 bg-yellow-500 text-black px-2 py-1 rounded-full text-sm font-bold">
                            <i class="fas fa-star"></i> ${movie.imdbScore.toFixed(1)}
                        </div>
                        <div class="absolute bottom-0 left-0 right-0 bg-gradient-to-t from-black to-transparent p-4">
                            <h3 class="font-bold text-lg truncate">${movie.title}</h3>
                            <p class="text-gray-400 text-sm">${movie.releaseYear} • ${movie.duration} min</p>
                            ${genresHtml}
                        </div>
                    </div>
                </div>
            `;
                    $('#trendingMoviesGrid').append(card);
                });

                skip += 10;
                button.prop('disabled', false).text('Load More Movies');
            },
            error: function () {
                button.prop('disabled', false).text('Error - Try Again');
            }
        });
    });
});

// Global function to load content modal
function loadContentModal(id, type) {
    $.ajax({
        url: `/${type}/GetContentModal`,
        type: 'GET',
        data: { id: id },
        success: function (html) {
            // Insert the HTML
            $('#contentModalContainer').html(html);

            // The modal has dynamic ID: contentModal-{id}
            const modal = $(`#contentModal-${id}`);

            if (modal.length > 0) {
                // Remove hidden class and show the modal
                modal.removeClass('hidden');
            }
        },
        error: function (xhr, status, error) {
            console.error('Failed to load content modal:', error);
            alert('Failed to load content details. Please try again.');
        }
    });
}

// Global function to close content modal
function closeContentModal(id) {
    $(`#contentModal-${id}`).addClass('hidden');
}