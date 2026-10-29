using System.Security.Claims;
using AlbumsWebApp.Contracts;
using AlbumsWebApp.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AlbumsWebApp.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AdminAuthenticationService authenticationService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    [EnableRateLimiting("admin-login")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(AdminLoginRequest request)
    {
        if (!authenticationService.VerifyCredentials(request.Email, request.Password))
            return Unauthorized(new ProblemDetails { Title = "Invalid email or password." });

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, authenticationService.Email),
            new Claim(ClaimTypes.Name, authenticationService.Email)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return NoContent();
    }

    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Me() => Ok(new { email = authenticationService.Email });

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }
}
