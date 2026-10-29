using AlbumsWebApp.Data;
using AlbumsWebApp.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AlbumsWebApp.Tests;

internal sealed class AlbumsWebApplicationFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@example.com";
    public const string AdminPassword = "correct-horse-battery-staple";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var passwordHash = new PasswordHasher<string>().HashPassword(AdminEmail, AdminPassword);
        var databaseName = $"albums-tests-{Guid.NewGuid()}";
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:AlbumsDb"] = "Host=localhost;Database=albums-test",
                ["ADMIN_EMAIL"] = AdminEmail,
                ["ADMIN_PASSWORD_HASH"] = passwordHash
            }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<AlbumsDbContext>();
            services.RemoveAll<DbContextOptions<AlbumsDbContext>>();
            services.AddDbContext<AlbumsDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
            services.RemoveAll<AdminAuthenticationService>();
            services.AddSingleton(new AdminAuthenticationService(AdminEmail, passwordHash));
        });
    }
}
