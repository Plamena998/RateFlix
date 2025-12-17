using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RateFlix.Core;
using RateFlix.Data;
using RateFlix.Data.Models;
using RateFlix.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ===== Configure services =====

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// HttpClient for TMDb service
builder.Services.AddHttpClient<TmdbService>();

// AppOptions from configuration
builder.Services.Configure<AppOptions>(builder.Configuration.GetSection("AppOptions"));
builder.Services.AddSingleton(resolver =>
    resolver.GetRequiredService<IOptions<AppOptions>>().Value);

// MVC
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


var app = builder.Build();

// ===== Apply migrations and seed data =====
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    // Run migrations
    var context = services.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();

    // Seed data
    await DataSeed.Initialize(services);
}

// ===== Configure middleware =====
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Razor Pages (if any)
app.MapRazorPages();

// Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Run application
await app.RunAsync();
