using AlbumsWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AlbumsWebApp.Data;

public sealed class AlbumsDbContext(DbContextOptions<AlbumsDbContext> options) : DbContext(options)
{
    public DbSet<Album> Albums => Set<Album>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var album = modelBuilder.Entity<Album>();

        album.Property(item => item.Title).HasMaxLength(120).IsRequired();
        album.Property(item => item.Artist).HasMaxLength(120).IsRequired();
        album.Property(item => item.Format).HasMaxLength(80).IsRequired();
        album.Property(item => item.Genre).HasMaxLength(80).IsRequired();
        album.Property(item => item.Condition).HasMaxLength(120).IsRequired();
        album.Property(item => item.Notes).HasMaxLength(500).IsRequired();

        album.HasData(
            new Album
            {
                Id = 1,
                Title = "Kind of Blue",
                Artist = "Miles Davis",
                ReleaseYear = 1959,
                Format = "Vinyl",
                Genre = "Jazz",
                Condition = "Very good",
                IsFavorite = true,
                Notes = "Classic modal jazz pressing."
            },
            new Album
            {
                Id = 2,
                Title = "Hounds of Love",
                Artist = "Kate Bush",
                ReleaseYear = 1985,
                Format = "CD",
                Genre = "Art pop",
                Condition = "Good",
                IsFavorite = false,
                Notes = "Keep near the 80s shelf."
            });
    }
}
