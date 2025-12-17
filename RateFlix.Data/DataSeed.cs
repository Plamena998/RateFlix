using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RateFlix.Data;
using RateFlix.Data.Models;
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

        // ===== Roles =====
        string[] roles = { "Administrator", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // ===== Admin =====
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

        // ===== Genres =====
        if (!context.Genres.Any())
        {
            context.Genres.AddRange(
                new Genre { Name = "Action" },
                new Genre { Name = "Adventure" },
                new Genre { Name = "Animation" },
                new Genre { Name = "Drama" },
                new Genre { Name = "Comedy" },
                new Genre { Name = "Crime" },
                new Genre { Name = "Sci-Fi" },
                new Genre { Name = "Thriller" }
            );
            await context.SaveChangesAsync();
        }

        var genres = context.Genres.ToList();

        // ===== Directors =====
        if (!context.Directors.Any())
        {
            context.Directors.AddRange(
                new Director { Name = "Christopher Nolan" },
                new Director { Name = "Quentin Tarantino" },
                new Director { Name = "Steven Spielberg" },
                new Director { Name = "Martin Scorsese" },
                new Director { Name = "Denis Villeneuve" }
            );
            await context.SaveChangesAsync();
        }

        var directors = context.Directors.ToList();

        // ===== Actors =====
        if (!context.Actors.Any())
        {
            var actors = new List<Actor>
            {
                new Actor { Name = "Leonardo DiCaprio", BirthDate = new DateTime(1974,11,11) },
                new Actor { Name = "Brad Pitt", BirthDate = new DateTime(1963,12,18) },
                new Actor { Name = "Robert De Niro", BirthDate = new DateTime(1943,8,17) },
                new Actor { Name = "Morgan Freeman", BirthDate = new DateTime(1937,6,1) },
                new Actor { Name = "Scarlett Johansson", BirthDate = new DateTime(1984,11,22) },
                new Actor { Name = "Tom Hanks", BirthDate = new DateTime(1956,7,9) },
                new Actor { Name = "Natalie Portman", BirthDate = new DateTime(1981,6,9) },
                new Actor { Name = "Christian Bale", BirthDate = new DateTime(1974,1,30) },
                new Actor { Name = "Emma Stone", BirthDate = new DateTime(1988,11,6) },
                new Actor { Name = "Matt Damon", BirthDate = new DateTime(1970,10,8) },
                new Actor { Name = "Anne Hathaway", BirthDate = new DateTime(1982,11,12) },
                new Actor { Name = "Samuel L. Jackson", BirthDate = new DateTime(1948,12,21) },
                new Actor { Name = "Johnny Depp", BirthDate = new DateTime(1963,6,9) },
                new Actor { Name = "Kate Winslet", BirthDate = new DateTime(1975,10,5) },
                new Actor { Name = "Hugh Jackman", BirthDate = new DateTime(1968,10,12) },
                new Actor { Name = "Chris Hemsworth", BirthDate = new DateTime(1983,8,11) },
                new Actor { Name = "Gal Gadot", BirthDate = new DateTime(1985,4,30) },
                new Actor { Name = "Ryan Reynolds", BirthDate = new DateTime(1976,10,23) },
                new Actor { Name = "Harrison Ford", BirthDate = new DateTime(1942,7,13) },
                new Actor { Name = "Anne-Marie Duff", BirthDate = new DateTime(1970,10,8) }
            };
            context.Actors.AddRange(actors);
            await context.SaveChangesAsync();
        }

        var actorsList = context.Actors.ToList();

        // ===== Movies =====
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

                    var trailerUrl = await tmdbService.GetMovieTrailerAsync(tmdb.Id)
                                     ?? "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

                    var movie = new Movie
                    {
                        Title = tmdb.Title,
                        Description = tmdb.Overview,
                        ReleaseYear = releaseDate.Year,
                        IMDBScore = Math.Round(Random.Shared.NextDouble() * 9 + 1, 1),
                        MetaScore = Random.Shared.Next(1, 101),
                        Duration = Random.Shared.Next(80, 181),
                        DirectorId = directors[added % directors.Count].Id,
                        ImageUrl = $"https://image.tmdb.org/t/p/w500{tmdb.Poster_path}",
                        TrailerUrl = trailerUrl,
                        ContentType = "Movie"
                    };

                    context.Movies.Add(movie);
                    await context.SaveChangesAsync();

                    foreach (var genreId in tmdb.Genre_ids.Take(2))
                    {
                        var genre = genres.FirstOrDefault(g => g.Id == genreId);
                        if (genre != null)
                            context.ContentGenres.Add(new ContentGenre { ContentId = movie.Id, GenreId = genre.Id });
                    }

                    foreach (var actor in actorsList.OrderBy(a => Guid.NewGuid()).Take(3))
                        context.ContentActors.Add(new ContentActor { ContentId = movie.Id, ActorId = actor.Id });

                    added++;
                }
                page++;
            }
            await context.SaveChangesAsync();
        }

        // ===== Series with Seasons and Episodes =====
        if (!context.Series.Any())
        {
            int page = 1, added = 0;
            var tmdbSeries = await tmdbService.GetSeriesAsync(page);

            foreach (var tmdb in tmdbSeries.Take(30))
            {
                if (!DateTime.TryParse(tmdb.First_air_date, out var airDate)) continue;

                var trailerUrl = await tmdbService.GetSeriesTrailerAsync(tmdb.Id)
                                 ?? "https://www.youtube.com/watch?v=dQw4w9WgXcQ";

                int totalSeasons = Random.Shared.Next(1, 6);

                var series = new Series
                {
                    Title = tmdb.Name,
                    Description = tmdb.Overview,
                    ReleaseYear = airDate.Year,
                    IMDBScore = 0, // Ще се изчисли след епизодите
                    MetaScore = 0, // Ще се изчисли след епизодите
                    DirectorId = directors[added % directors.Count].Id,
                    ImageUrl = $"https://image.tmdb.org/t/p/w500{tmdb.Poster_path}",
                    TrailerUrl = trailerUrl,
                    ContentType = "Series",
                    TotalSeasons = totalSeasons
                };

                context.Series.Add(series);
                await context.SaveChangesAsync();

                foreach (var genreId in tmdb.Genre_ids.Take(2))
                {
                    var genre = genres.FirstOrDefault(g => g.Id == genreId);
                    if (genre != null)
                        context.ContentGenres.Add(new ContentGenre { ContentId = series.Id, GenreId = genre.Id });
                }

                foreach (var actor in actorsList.OrderBy(a => Guid.NewGuid()).Take(3))
                    context.ContentActors.Add(new ContentActor { ContentId = series.Id, ActorId = actor.Id });

                // ===== Създаваме сезони и епизоди =====
                for (int seasonNum = 1; seasonNum <= totalSeasons; seasonNum++)
                {
                    var season = new Season
                    {
                        SeasonNumber = seasonNum,
                        SeriesId = series.Id,
                        ReleaseYear = airDate.Year + (seasonNum - 1),
                        Description = $"Season {seasonNum} of {series.Title}",
                        IMDBScore = 0, // Ще се изчисли след епизодите
                        MetaScore = 0  // Ще се изчисли след епизодите
                    };

                    context.Seasons.Add(season);
                    await context.SaveChangesAsync();

                    int episodesCount = Random.Shared.Next(6, 13);

                    for (int epNum = 1; epNum <= episodesCount; epNum++)
                    {
                        context.Episodes.Add(new Episode
                        {
                            Title = $"S{seasonNum:D2}E{epNum:D2} - Episode {epNum}",
                            SeasonId = season.Id,
                            IMDBScore = Math.Round(Random.Shared.NextDouble() * 9 + 1, 1),
                            MetaScore = Random.Shared.Next(1, 101),
                            EpisodeNumber = epNum,
                            Duration = Random.Shared.Next(40, 61),
                            TrailerUrl = trailerUrl
                        });
                    }

                    await context.SaveChangesAsync();

                    // Изчисляваме оценки на сезона след добавяне на епизодите
                    var episodes = context.Episodes.Where(e => e.SeasonId == season.Id).ToList();
                    season.IMDBScore = Math.Round(episodes.Average(e => e.IMDBScore), 1);
                    season.MetaScore = Math.Round(episodes.Average(e => e.MetaScore), 1);
                }

                await context.SaveChangesAsync();

                // Изчисляваме оценки на сериала след добавяне на всички сезони
                var seasons = context.Seasons.Where(s => s.SeriesId == series.Id).ToList();
                series.IMDBScore = Math.Round(seasons.Average(s => s.IMDBScore), 1);
                series.MetaScore = Math.Round(seasons.Average(s => s.MetaScore), 1);

                await context.SaveChangesAsync();
                added++;
            }
        }
    }
}