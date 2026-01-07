using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RateFlix.Data.Models;
using RateFlix.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RateFlix.Data
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
           : base(options)
        {

        }
        public DbSet<Movie> Movies { get; set; } = null!;
        public DbSet<Series> Series { get; set; } = null!;
        public DbSet<Season> Seasons { get; set; } = null!;
        public DbSet<Episode> Episodes { get; set; } = null!;
        public DbSet<Genre> Genres { get; set; } = null!;
        public DbSet<ContentGenre> ContentGenres { get; set; } = null!;
        public DbSet<Director> Directors { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<FavoriteContent> FavoriteContentя { get; set; } = null!;
        public DbSet<Actor> Actors { get; set; } = null!;
        public DbSet<ContentActor> ContentActors { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Content>().UseTptMappingStrategy();

            modelBuilder.Entity<Movie>().ToTable("Movies");
            modelBuilder.Entity<Series>().ToTable("Series");

            modelBuilder.Entity<Movie>()
                .HasOne(m => m.Director)
                .WithMany(d => d.Movies)
                .HasForeignKey(m => m.DirectorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Series>()
                .HasOne(s => s.Director)
                .WithMany(d => d.Series)
                .HasForeignKey(s => s.DirectorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ContentGenre>()
                .HasKey(cg => new { cg.ContentId, cg.GenreId });

            modelBuilder.Entity<ContentGenre>()
                .HasOne(cg => cg.Content)
                .WithMany(c => c.ContentGenres)
                .HasForeignKey(cg => cg.ContentId);

            modelBuilder.Entity<ContentGenre>()
                .HasOne(cg => cg.Genre)
                .WithMany(g => g.ContentGenres)
                .HasForeignKey(cg => cg.GenreId);

            modelBuilder.Entity<Season>()
                .HasOne(s => s.Series)
                .WithMany(sr => sr.Seasons)
                .HasForeignKey(s => s.SeriesId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Episode>()
                .HasOne(e => e.Season)
                .WithMany(s => s.Episodes)
                .HasForeignKey(e => e.SeasonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.Content)
                .WithMany(c => c.Reviews)
                .HasForeignKey(r => r.ContentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<FavoriteContent>()
                .HasKey(f => new { f.UserId, f.ContentId });

            modelBuilder.Entity<FavoriteContent>()
                .HasOne(f => f.User)
                .WithMany(u => u.FavoriteMovies)
                .HasForeignKey(f => f.UserId);

            modelBuilder.Entity<FavoriteContent>()
                .HasOne(f => f.Content)
                .WithMany(c => c.FavoriteMovies)
                .HasForeignKey(f => f.ContentId);

            modelBuilder.Entity<ContentActor>()
                .HasKey(ca => new { ca.ContentId, ca.ActorId });

            modelBuilder.Entity<ContentActor>()
                .HasOne(ca => ca.Content)
                .WithMany(c => c.ContentActors)
                .HasForeignKey(ca => ca.ContentId);

            modelBuilder.Entity<ContentActor>()
                .HasOne(ca => ca.Actor)
                .WithMany(a => a.ContentActors)
                .HasForeignKey(ca => ca.ActorId);

            modelBuilder.Entity<Review>()
                .HasIndex(r => new { r.ContentId, r.UserId });

            modelBuilder.Entity<Season>()
                .HasIndex(s => s.SeriesId);
        }
    }
}