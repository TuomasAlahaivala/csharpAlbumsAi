using AlbumsWebApp.Services;
using Microsoft.AspNetCore.Identity;

namespace AlbumsWebApp.Tests;

public sealed class AdminAuthenticationServiceTests
{
    private const string Email = "admin-123@example.com";
    private const string Password = "a-strong-password";

    [Fact]
    public void VerifyCredentials_accepts_configured_email_and_password()
    {
        var service = CreateService();

        Assert.True(service.VerifyCredentials(Email, Password));
        Assert.True(service.VerifyCredentials(Email.ToUpperInvariant(), Password));
    }

    [Fact]
    public void VerifyCredentials_rejects_wrong_email_or_password()
    {
        var service = CreateService();

        Assert.False(service.VerifyCredentials("admin-other@example.com", Password));
        Assert.False(service.VerifyCredentials(Email, "wrong-password"));
    }

    private static AdminAuthenticationService CreateService()
    {
        var passwordHash = new PasswordHasher<string>().HashPassword(Email, Password);
        return new AdminAuthenticationService(Email, passwordHash);
    }
}
