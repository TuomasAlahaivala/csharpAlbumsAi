using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;

namespace AlbumsWebApp.Services;

internal static class AdminCredentialInitializer
{
    public static void Initialize(string directory)
    {
        var path = Path.Combine(directory, ".env");
        if (Environment.GetEnvironmentVariable("ADMIN_EMAIL") is not null
            || Environment.GetEnvironmentVariable("ADMIN_PASSWORD_HASH") is not null)
            throw new InvalidOperationException("Admin credentials are already supplied through environment variables.");
        if (File.Exists(path))
            throw new InvalidOperationException(".env already exists. Remove it only if you intend to generate a new admin account.");

        var emailId = Convert.ToHexString(RandomNumberGenerator.GetBytes(5)).ToLowerInvariant();
        var email = $"admin-{emailId}@example.com";
        var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
        var passwordHash = new PasswordHasher<string>().HashPassword(email, password);
        var contents = $"ADMIN_EMAIL={email}{Environment.NewLine}ADMIN_PASSWORD_HASH={passwordHash}{Environment.NewLine}";

        using (var file = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        using (var writer = new StreamWriter(file))
            writer.Write(contents);

        Console.WriteLine("Admin credentials created. Save the password now; it cannot be recovered.");
        Console.WriteLine($"Email: {email}");
        Console.WriteLine($"Password: {password}");
        Console.WriteLine("The password hash was saved to .env.");
    }
}
