using AlbumsWebApp.Data;
using AlbumsWebApp.Models;
using AlbumsWebApp.Services;
using Microsoft.EntityFrameworkCore;

namespace AlbumsWebApp.Tests;

public sealed class AlbumStoreTests
{
    [Fact]
    public async Task Search_filters_and_orders_albums()
    {
        await using var context = CreateContext();
        context.Albums.AddRange(
            Album("Second", "Beta", "Jazz", "CD", false),
            Album("First", "Alpha", "Jazz", "Vinyl", true),
            Album("Other", "Gamma", "Rock", "Vinyl", false));
        await context.SaveChangesAsync();

        var result = await new AlbumStore(context).SearchAsync("jazz", null);

        Assert.Equal(["First", "Second"], result.Albums.Select(album => album.Title));
        Assert.Equal(["CD", "Vinyl"], result.AvailableFormats);
    }

    [Fact]
    public async Task Update_and_delete_handle_existing_and_missing_records()
    {
        await using var context = CreateContext();
        var store = new AlbumStore(context);
        var existing = Album("Before", "Artist", "Genre", "CD", false);
        await store.AddAsync(existing);

        var updated = Album("After", "Artist", "Genre", "Vinyl", true);
        updated.Id = existing.Id;

        Assert.True(await store.UpdateAsync(updated));
        Assert.Equal("After", (await store.GetByIdAsync(existing.Id))?.Title);
        Assert.True(await store.DeleteAsync(existing.Id));
        Assert.Null(await store.GetByIdAsync(existing.Id));
        Assert.False(await store.UpdateAsync(Album("Missing", "Artist", "Genre", "CD", false)));
        Assert.False(await store.DeleteAsync(999));
    }

    [Fact]
    public async Task Add_and_get_round_trip_an_album()
    {
        await using var context = CreateContext();
        var store = new AlbumStore(context);
        var album = Album("New Album", "Artist", "Rock", "Vinyl", true);

        await store.AddAsync(album);
        var saved = await store.GetByIdAsync(album.Id);

        Assert.NotNull(saved);
        Assert.Equal("New Album", saved.Title);
        Assert.True(saved.IsFavorite);
    }

    private static AlbumsDbContext CreateContext() => new(
        new DbContextOptionsBuilder<AlbumsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    private static Album Album(string title, string artist, string genre, string format, bool favorite) => new()
    {
        Title = title,
        Artist = artist,
        ReleaseYear = 2000,
        Genre = genre,
        Format = format,
        Condition = "Good",
        IsFavorite = favorite
    };
}
