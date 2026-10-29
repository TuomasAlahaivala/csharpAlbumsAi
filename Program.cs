using AlbumsWebApp.Data;
using AlbumsWebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var initializeAdmin = args.Contains("--initialize-admin", StringComparer.Ordinal);
var builderArgs = args.Where(argument => argument != "--initialize-admin").ToArray();
var builder = WebApplication.CreateBuilder(builderArgs);
DotEnv.Load(builder.Environment.ContentRootPath);
builder.Configuration.AddEnvironmentVariables();

if (initializeAdmin)
{
    AdminCredentialInitializer.Initialize(builder.Environment.ContentRootPath);
    return;
}

builder.Services.AddControllers();
builder.Services.AddDbContext<AlbumsDbContext>((services, options) =>
{
    var connectionString = services.GetRequiredService<IConfiguration>().GetConnectionString("AlbumsDb")
        ?? throw new InvalidOperationException("Connection string 'AlbumsDb' is not configured.");
    options.UseNpgsql(connectionString);
});
builder.Services.AddScoped<AlbumStore>();
builder.Services.AddSingleton(services =>
{
    var configuration = services.GetRequiredService<IConfiguration>();
    var adminEmail = configuration["ADMIN_EMAIL"]
        ?? throw new InvalidOperationException("ADMIN_EMAIL is not configured. Run the application with --initialize-admin.");
    var adminPasswordHash = configuration["ADMIN_PASSWORD_HASH"]
        ?? throw new InvalidOperationException("ADMIN_PASSWORD_HASH is not configured. Run the application with --initialize-admin.");
    return new AdminAuthenticationService(adminEmail, adminPasswordHash);
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AlbumAuth";
        options.Cookie.HttpOnly = true;
        options.Cookie.Path = "/";
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
        options.Events.OnRedirectToAccessDenied = context =>
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("admin-login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AlbumsDbContext>();
    if (dbContext.Database.IsRelational())
        await dbContext.Database.MigrateAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers().RequireAuthorization();
app.Map("/api/{**path}", () => Results.Problem(
    statusCode: StatusCodes.Status404NotFound,
    title: "API endpoint not found.")).RequireAuthorization();
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
