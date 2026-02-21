using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RateFlix.Core;
using RateFlix.Core.Models;
using RateFlix.Data;
using RateFlix.Middleware;
using RateFlix.Services;
using RateFlix.Services.Interfaces;
var builder = WebApplication.CreateBuilder(args);


// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(x => { x.Password.RequiredLength = 12; })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // all JSON responses use camelCase
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.AddHttpClient<TmdbService>();

builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IActorsService, ActorsService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<ISeriesService, SeriesService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IAdminMovieService, AdminMovieService>();
builder.Services.AddScoped<IAdminSeriesService, AdminSeriesService>();
builder.Services.AddScoped<IAdminUsersService, AdminUsersService>();
builder.Services.AddScoped<IAdminReviewsService, AdminReviewsService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();


// AppOptions
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("AppOptions"));
builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<IOptions<AppOptions>>().Value);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddTransient<IEmailSender, EmailSender>();


var app = builder.Build();

// ===== Apply migrations and seed data =====
//using (var scope = app.Services.CreateScope())
//{
//    var services = scope.ServiceProvider;

//    // Run migrations
//    var context = services.GetRequiredService<AppDbContext>();
//    await context.Database.MigrateAsync();

//    // Seed data
//    await DataSeed.Initialize(services);
//}
//Console.WriteLine("gotovo");

//  middleware 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseMiddleware<IPBlacklistMiddleware>("access-denied.html");

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

await app.RunAsync();
