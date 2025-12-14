using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RateFlix.Data;
using RateFlix.Infrastructure;

public class DataSeed
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        //роли
        string[] roles = new[] { "Administrator", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // Администратор
        var adminEmail = "admin@rateflix.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new AppUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
            await userManager.CreateAsync(adminUser, "Admin123!"); //парола:Admin123!
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }
        // Жанрове
        if (!context.Genres.Any())
        {
            context.Genres.AddRange(
                new Genre { Name = "Action" },
                new Genre { Name = "Comedy" },
                new Genre { Name = "Drama" },
                new Genre { Name = "Thriller" },
                new Genre { Name = "Sci-Fi" }
            );
            context.SaveChanges();
        }

        //Режисьори
        if (!context.Directors.Any())
        {
            context.Directors.AddRange(
                new Director { Name = "Christopher Nolan" },
                new Director { Name = "Quentin Tarantino" },
                new Director { Name = "Steven Spielberg" },
                new Director { Name = "Martin Scorsese" }
            );
            context.SaveChanges();
        }

        // Филми (15/брой)
        if (!context.Movies.Any())
        {
            var directors = context.Directors.ToList();
            var genres = context.Genres.ToList();

            var movies = new List<Movie>
{
    new Movie { Title="Inception", DirectorId=directors[0].Id, IMDBScore=8.8m, MetaScore=74, Description="A thief who steals corporate secrets through dream-sharing technology is given a chance to erase his criminal history.", ReleaseYear=2010 },
    new Movie { Title="Interstellar", DirectorId=directors[0].Id, IMDBScore=8.6m, MetaScore=74, Description="A team of explorers travel through a wormhole in space in an attempt to ensure humanity's survival.", ReleaseYear=2014 },
    new Movie { Title="The Dark Knight", DirectorId=directors[0].Id, IMDBScore=9.0m, MetaScore=84, Description="Batman faces the Joker, a criminal mastermind who wants to plunge Gotham City into chaos.", ReleaseYear=2008 },
    new Movie { Title="Pulp Fiction", DirectorId=directors[1].Id, IMDBScore=8.9m, MetaScore=94, Description="The lives of two mob hitmen, a boxer, and others intertwine in a tale of violence and redemption.", ReleaseYear=1994 },
    new Movie { Title="Django Unchained", DirectorId=directors[1].Id, IMDBScore=8.4m, MetaScore=81, Description="With the help of a bounty hunter, a freed slave sets out to rescue his wife from a brutal plantation owner.", ReleaseYear=2012 },
    new Movie { Title="Inglourious Basterds", DirectorId=directors[1].Id, IMDBScore=8.3m, MetaScore=69, Description="In Nazi-occupied France, a group of Jewish soldiers plan to assassinate the leaders of the Third Reich.", ReleaseYear=2009 },
    new Movie { Title="Jurassic Park", DirectorId=directors[2].Id, IMDBScore=8.1m, MetaScore=68, Description="A theme park showcasing cloned dinosaurs goes terribly wrong when the creatures escape.", ReleaseYear=1993 },
    new Movie { Title="E.T.", DirectorId=directors[2].Id, IMDBScore=7.8m, MetaScore=91, Description="A troubled child befriends a gentle alien stranded on Earth and helps him return home.", ReleaseYear=1982 },
    new Movie { Title="Schindler's List", DirectorId=directors[2].Id, IMDBScore=8.9m, MetaScore=94, Description="The story of Oskar Schindler, who saved over a thousand Jews during the Holocaust.", ReleaseYear=1993 },
    new Movie { Title="The Wolf of Wall Street", DirectorId=directors[3].Id, IMDBScore=8.2m, MetaScore=75, Description="The rise and fall of stockbroker Jordan Belfort and his high-flying lifestyle.", ReleaseYear=2013 },
    new Movie { Title="Shutter Island", DirectorId=directors[3].Id, IMDBScore=8.1m, MetaScore=63, Description="Two U.S. Marshals investigate the disappearance of a patient from a mental institution.", ReleaseYear=2010 },
    new Movie { Title="Casino", DirectorId=directors[3].Id, IMDBScore=8.2m, MetaScore=82, Description="The rise and fall of a casino operator in Las Vegas with ties to the mob.", ReleaseYear=1995 },
    new Movie { Title="Tenet", DirectorId=directors[0].Id, IMDBScore=7.5m, MetaScore=69, Description="A secret agent embarks on a time-bending mission to prevent World War III.", ReleaseYear=2020 },
    new Movie { Title="Catch Me If You Can", DirectorId=directors[2].Id, IMDBScore=8.1m, MetaScore=75, Description="The story of Frank Abagnale Jr., who successfully committed fraud while evading the FBI.", ReleaseYear=2002 },
    new Movie { Title="Dunkirk", DirectorId=directors[0].Id, IMDBScore=7.9m, MetaScore=94, Description="Allied soldiers are evacuated from the beaches of Dunkirk before Nazi forces can capture them.", ReleaseYear=2017 }
};


            context.Movies.AddRange(movies);
            context.SaveChanges();

            // MovieGenre връзки (по едно-две жанра на филм)
            foreach (var movie in movies)
            {
                // Примерно задава жанр Action за първите няколко, Comedy за няколко и др.
                if (movie.Title.Contains("Inception") || movie.Title.Contains("Dark") || movie.Title.Contains("Tenet"))
                {
                    context.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genres.First(g => g.Name == "Action").Id });
                }
                if (movie.Title.Contains("Pulp") || movie.Title.Contains("Django"))
                {
                    context.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genres.First(g => g.Name == "Drama").Id });
                }
                if (movie.Title.Contains("E.T.") || movie.Title.Contains("Jurassic"))
                {
                    context.MovieGenres.Add(new MovieGenre { MovieId = movie.Id, GenreId = genres.First(g => g.Name == "Sci-Fi").Id });
                }
            }
            context.SaveChanges();
        }
    }
}
