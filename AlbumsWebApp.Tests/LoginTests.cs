using System.Net;
using System.Net.Http.Json;

namespace AlbumsWebApp.Tests;

public sealed class LoginTests
{
    [Fact]
    public async Task Login_rejects_bad_credentials_and_accepts_valid_credentials()
    {
        using var factory = new AlbumsWebApplicationFactory();
        using var client = factory.CreateClient();

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);

        var invalidLogin = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AlbumsWebApplicationFactory.AdminEmail,
            password = "wrong-password"
        });
        Assert.Equal(HttpStatusCode.Unauthorized, invalidLogin.StatusCode);

        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = AlbumsWebApplicationFactory.AdminEmail,
            password = AlbumsWebApplicationFactory.AdminPassword
        });
        Assert.Equal(HttpStatusCode.NoContent, login.StatusCode);

        var currentUser = await client.GetFromJsonAsync<Dictionary<string, string>>("/api/auth/me");
        Assert.NotNull(currentUser);
        Assert.Equal(AlbumsWebApplicationFactory.AdminEmail, currentUser["email"]);

        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/auth/logout", null)).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/auth/me")).StatusCode);
    }
}
