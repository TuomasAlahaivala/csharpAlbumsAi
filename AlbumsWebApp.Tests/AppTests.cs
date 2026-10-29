using System.Net;
using System.Net.Http.Json;
using AlbumsWebApp.Contracts;

namespace AlbumsWebApp.Tests;

public sealed class AppTests
{
    [Fact]
    public async Task Admin_can_create_view_update_search_and_delete_albums()
    {
        using var factory = new AlbumsWebApplicationFactory();
        using var client = factory.CreateClient();

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AlbumsWebApplicationFactory.AdminEmail,
            password = AlbumsWebApplicationFactory.AdminPassword
        });
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        var create = await client.PostAsJsonAsync("/api/albums", new AlbumRequest
        {
            Title = "Test Album",
            Artist = "Test Artist",
            ReleaseYear = 2024,
            Format = "Vinyl",
            Genre = "Jazz",
            Condition = "Good",
            IsFavorite = true,
            Notes = "Created by the app test."
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var createdAlbum = await create.Content.ReadFromJsonAsync<AlbumResponse>();
        Assert.NotNull(createdAlbum);
        Assert.True(createdAlbum.Id > 0);

        var get = await client.GetAsync($"/api/albums/{createdAlbum.Id}");
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal("Test Album", (await get.Content.ReadFromJsonAsync<AlbumResponse>())?.Title);

        var search = await client.GetFromJsonAsync<AlbumListResponse>("/api/albums?search=Test%20Artist");
        Assert.NotNull(search);
        Assert.Contains(search.Albums, album => album.Id == createdAlbum.Id);

        var update = await client.PutAsJsonAsync($"/api/albums/{createdAlbum.Id}", new AlbumRequest
        {
            Title = "Updated Album",
            Artist = "Test Artist",
            ReleaseYear = 2024,
            Format = "CD",
            Genre = "Jazz",
            Condition = "Very good",
            IsFavorite = false,
            Notes = string.Empty
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal("Updated Album", (await update.Content.ReadFromJsonAsync<AlbumResponse>())?.Title);

        Assert.Equal(HttpStatusCode.NoContent,
            (await client.DeleteAsync($"/api/albums/{createdAlbum.Id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync($"/api/albums/{createdAlbum.Id}")).StatusCode);
    }
}
