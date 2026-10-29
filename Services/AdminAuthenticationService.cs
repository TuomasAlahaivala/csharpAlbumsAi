using Microsoft.AspNetCore.Identity;

namespace AlbumsWebApp.Services;

public sealed class AdminAuthenticationService
{
    private readonly AdminCredential _credential;
    private readonly string _passwordHash;
    private readonly PasswordHasher<AdminCredential> _passwordHasher = new();

    public AdminAuthenticationService(string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new InvalidOperationException("ADMIN_EMAIL must be configured.");
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new InvalidOperationException("ADMIN_PASSWORD_HASH must be configured.");

        _credential = new AdminCredential(email);
        _passwordHash = passwordHash;
    }

    public string Email => _credential.Email;

    public bool VerifyCredentials(string email, string password)
    {
        var passwordIsValid = _passwordHasher.VerifyHashedPassword(_credential, _passwordHash, password)
            != PasswordVerificationResult.Failed;
        return passwordIsValid && string.Equals(email, _credential.Email, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record AdminCredential(string Email);
}
