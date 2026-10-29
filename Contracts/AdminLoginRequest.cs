using System.ComponentModel.DataAnnotations;

namespace AlbumsWebApp.Contracts;

/// <summary>Credentials used to sign in to the configured admin account.</summary>
public sealed record AdminLoginRequest
{
    [Required, EmailAddress, MaxLength(320)]
    public required string Email { get; init; }

    [Required, MaxLength(1024)]
    public required string Password { get; init; }
}
