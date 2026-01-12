using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RateFlix.Core.Models;
using RateFlix.Data;
using RateFlix.Infrastructure;
using System.Globalization;

public static class DataSeed
{
    public static async Task Initialize(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var tmdbService = scope.ServiceProvider.GetRequiredService<TmdbService>();

        await context.Database.MigrateAsync();

        string[] roles = { "Administrator", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        var adminEmail = "admin@rateflix.com";
        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new AppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, "Admin123!");
            await userManager.AddToRoleAsync(admin, "Administrator");
        }

        if (!context.Genres.Any())
        {
            var movieGenres = await tmdbService.GetMovieGenresAsync();
            var tvGenres = await tmdbService.GetTvGenresAsync();

            //genres without duplicates
            var allGenres = movieGenres
                .Concat(tvGenres)
                .GroupBy(g => g.Name)
                .Select(g => g.First())
                .ToList();

            foreach (var tmdbGenre in allGenres)
            {
                context.Genres.Add(new Genre
                {
                    Name = tmdbGenre.Name
                });
            }
            await context.SaveChangesAsync();
        }

        var genres = context.Genres.ToList();

        // dictionaries to cache actors and directors
        var actorCache = new Dictionary<int, Actor>();
        var directorCache = new Dictionary<int, Director>();

        if (!context.Movies.Any())
        {
            int page = 1, added = 0;

            while (added < 100)
            {
                var tmdbMovies = await tmdbService.GetMoviesAsync(page);

                foreach (var tmdb in tmdbMovies)
                {
                    if (added >= 100) break;
                    if (!DateTime.TryParse(tmdb.Release_date, out var releaseDate)) continue;

                    try
                    {
                        var trailerUrl = await tmdbService.GetMovieTrailerAsync(tmdb.Id)
                                         ?? "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

                        var credits = await tmdbService.GetMovieCreditsAsync(tmdb.Id);

                        // get durectors => credits
                        var directorCrew = credits?.Crew?.FirstOrDefault(c => c.Job == "Director");
                        Director? director = null;

                        if (directorCrew != null)
                        {
                            if (!directorCache.ContainsKey(directorCrew.Id))
                            {
                                director = new Director
                                {
                                    Name = directorCrew.Name,
                                    ImageUrl = directorCrew.Profile_path != null
                                        ? $"https://image.tmdb.org/t/p/w500{directorCrew.Profile_path}"
                                        : "/images/directors/default.jpg"
                                };
                                context.Directors.Add(director);
                                await context.SaveChangesAsync();
                                directorCache[directorCrew.Id] = director;
                            }
                            else
                            {
                                director = directorCache[directorCrew.Id];
                            }
                        }

                        if (director == null) continue; 

                        var movie = new Movie
                        {
                            Title = tmdb.Title,
                            Description = tmdb.Overview,
                            ReleaseYear = releaseDate.Year,
                            IMDBScore = Math.Round((double)tmdb.Vote_average, 1),
                            MetaScore = Random.Shared.Next(1, 101),
                            Duration = Random.Shared.Next(80, 181),
                            DirectorId = director.Id,
                            ImageUrl = $"https://image.tmdb.org/t/p/w500{tmdb.Poster_path}",
                            TrailerUrl = trailerUrl,
                            ContentType = "Movie"
                        };

                        context.Movies.Add(movie);
                        await context.SaveChangesAsync();

                        // add genres (top 3)
                        foreach (var genreId in tmdb.Genre_ids.Take(3))
                        {
                            var genre = genres.FirstOrDefault(g => g.Name == GetGenreName(genreId));
                            if (genre != null)
                            {
                                context.ContentGenres.Add(new ContentGenre
                                {
                                    ContentId = movie.Id,
                                    GenreId = genre.Id
                                });
                            }
                        }

                        // add actors (top 5)
                        if (credits?.Cast != null)
                        {
                            foreach (var tmdbActor in credits.Cast.Take(5))
                            {
                                if (!actorCache.ContainsKey(tmdbActor.Id))
                                {
                                    var personDetails = await tmdbService.GetPersonDetailsAsync(tmdbActor.Id);

                                    var actor = new Actor
                                    {
                                        Name = tmdbActor.Name,
                                        BirthDate = DateTime.TryParse(personDetails?.Birthday, out var bday)
                                            ? bday
                                            : new DateTime(1980, 1, 1),
                                        ImageUrl = tmdbActor.Profile_path != null
                                            ? $"https://image.tmdb.org/t/p/w500{tmdbActor.Profile_path}"
                                            : "/images/actors/default.jpg"
                                    };
                                    context.Actors.Add(actor);
                                    await context.SaveChangesAsync();
                                    actorCache[tmdbActor.Id] = actor;
                                }

                                context.ContentActors.Add(new ContentActor
                                {
                                    ContentId = movie.Id,
                                    ActorId = actorCache[tmdbActor.Id].Id
                                });
                            }
                        }

                        await context.SaveChangesAsync();
                        added++;

                        // Rate limiting - avoid hitting TMDB API limits
                        await Task.Delay(250);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing movie {tmdb.Title}: {ex.Message}");
                        continue;
                    }
                }
                page++;
            }
        }

        // ===== Series with Seasons and Episodes =====
        if (!context.Series.Any())
        {
            int page = 1, added = 0;

            while (added < 30)
            {
                var tmdbSeriesList = await tmdbService.GetSeriesAsync(page);

                foreach (var tmdb in tmdbSeriesList)
                {
                    if (added >= 30) break;
                    if (!DateTime.TryParse(tmdb.First_air_date, out var airDate)) continue;

                    try
                    {
                        var seriesDetails = await tmdbService.GetSeriesDetailsAsync(tmdb.Id);
                        if (seriesDetails == null) continue;

                        var trailerUrl = await tmdbService.GetSeriesTrailerAsync(tmdb.Id)
                                         ?? "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

                        var credits = await tmdbService.GetSeriesCreditsAsync(tmdb.Id);

                        // get director
                        Director? director = null;
                        var creator = seriesDetails.Created_by?.FirstOrDefault();

                        if (creator != null)
                        {
                            if (!directorCache.ContainsKey(creator.Id))
                            {
                                director = new Director
                                {
                                    Name = creator.Name,
                                    ImageUrl = creator.Profile_path != null
                                        ? $"https://image.tmdb.org/t/p/w500{creator.Profile_path}"
                                        : "/images/directors/default.jpg"
                                };
                                context.Directors.Add(director);
                                await context.SaveChangesAsync();
                                directorCache[creator.Id] = director;
                            }
                            else
                            {
                                director = directorCache[creator.Id];
                            }
                        }

                        if (director == null) continue;

                        var series = new Series
                        {
                            Title = seriesDetails.Name,
                            Description = seriesDetails.Overview,
                            ReleaseYear = airDate.Year,
                            IMDBScore = 0, // calculated after seasons and episodes are added
                            MetaScore = 0,
                            DirectorId = director.Id,
                            ImageUrl = $"https://image.tmdb.org/t/p/w500{seriesDetails.Poster_path}",
                            TrailerUrl = trailerUrl,
                            ContentType = "Series",
                            TotalSeasons = seriesDetails.Number_of_seasons
                        };

                        context.Series.Add(series);
                        await context.SaveChangesAsync();

                        // add genres (top 3)
                        foreach (var genre in seriesDetails.Genres.Take(3))
                        {
                            var dbGenre = genres.FirstOrDefault(g => g.Name == genre.Name);
                            if (dbGenre != null)
                            {
                                context.ContentGenres.Add(new ContentGenre
                                {
                                    ContentId = series.Id,
                                    GenreId = dbGenre.Id
                                });
                            }
                        }

                        // add actors (top 5)
                        if (credits?.Cast != null)
                        {
                            foreach (var tmdbActor in credits.Cast.Take(5))
                            {
                                if (!actorCache.ContainsKey(tmdbActor.Id))
                                {
                                    var personDetails = await tmdbService.GetPersonDetailsAsync(tmdbActor.Id);

                                    var actor = new Actor
                                    {
                                        Name = tmdbActor.Name,
                                        BirthDate = DateTime.TryParse(personDetails?.Birthday, out var bday)
                                            ? bday
                                            : new DateTime(1980, 1, 1),
                                        ImageUrl = tmdbActor.Profile_path != null
                                            ? $"https://image.tmdb.org/t/p/w500{tmdbActor.Profile_path}"
                                            : "/images/actors/default.jpg"
                                    };
                                    context.Actors.Add(actor);
                                    await context.SaveChangesAsync();
                                    actorCache[tmdbActor.Id] = actor;
                                }

                                context.ContentActors.Add(new ContentActor
                                {
                                    ContentId = series.Id,
                                    ActorId = actorCache[tmdbActor.Id].Id
                                });
                            }
                        }

                        await context.SaveChangesAsync();

                        // ===== seasons and episodes =====
                        var seasonsToLoad = Math.Min(seriesDetails.Number_of_seasons, 3); 

                        for (int seasonNum = 1; seasonNum <= seasonsToLoad; seasonNum++)
                        {
                            var tmdbSeason = await tmdbService.GetSeasonDetailsAsync(tmdb.Id, seasonNum);
                            if (tmdbSeason == null) continue;

                            var season = new Season
                            {
                                SeasonNumber = seasonNum,
                                SeriesId = series.Id,
                                ReleaseYear = DateTime.TryParse(tmdbSeason.Air_date, out var sAirDate)
                                    ? sAirDate.Year
                                    : airDate.Year + (seasonNum - 1),
                                Description = tmdbSeason.Overview,
                                IMDBScore = 0,
                                MetaScore = 0
                            };

                            context.Seasons.Add(season);
                            await context.SaveChangesAsync();

                            foreach (var tmdbEpisode in tmdbSeason.Episodes)
                            {
                                context.Episodes.Add(new Episode
                                {
                                    Title = tmdbEpisode.Name,
                                    SeasonId = season.Id,
                                    IMDBScore = Math.Round(tmdbEpisode.Vote_average, 1),
                                    MetaScore = Random.Shared.Next(1, 101),
                                    EpisodeNumber = tmdbEpisode.Episode_number,
                                    Duration = tmdbEpisode.Runtime > 0 ? tmdbEpisode.Runtime : 45,
                                    TrailerUrl = trailerUrl
                                });
                            }

                            await context.SaveChangesAsync();

                            // calculating season scores
                            var episodes = context.Episodes.Where(e => e.SeasonId == season.Id).ToList();
                            if (episodes.Any())
                            {
                                season.IMDBScore = Math.Round(episodes.Average(e => e.IMDBScore), 1);
                                season.MetaScore = Math.Round(episodes.Average(e => e.MetaScore), 1);
                            }

                            await Task.Delay(250); // Rate limiting
                        }

                        await context.SaveChangesAsync();

                        // calculating series scores
                        var seasons = context.Seasons.Where(s => s.SeriesId == series.Id).ToList();
                        if (seasons.Any())
                        {
                            series.IMDBScore = Math.Round(seasons.Average(s => s.IMDBScore), 1);
                            series.MetaScore = Math.Round(seasons.Average(s => s.MetaScore), 1);
                        }

                        await context.SaveChangesAsync();
                        added++;

                        await Task.Delay(500); // Rate limiting between series
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing series {tmdb.Name}: {ex.Message}");
                        continue;
                    }
                }
                page++;
            }
        }
    }

    // Helper метод for map genre ID към име (TMDB genre IDs)
    private static string GetGenreName(int genreId)
    {
        return genreId switch
        {
            28 => "Action",
            12 => "Adventure",
            16 => "Animation",
            35 => "Comedy",
            80 => "Crime",
            99 => "Documentary",
            18 => "Drama",
            10751 => "Family",
            14 => "Fantasy",
            36 => "History",
            27 => "Horror",
            10402 => "Music",
            9648 => "Mystery",
            10749 => "Romance",
            878 => "Sci-Fi",
            10770 => "TV Movie",
            53 => "Thriller",
            10752 => "War",
            37 => "Western",
            _ => "Unknown"
        };
    }
}