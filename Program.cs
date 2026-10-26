using AlbumsWebApp.Data;
using AlbumsWebApp.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("AlbumsDb")
    ?? throw new InvalidOperationException("Connection string 'AlbumsDb' is not configured.");

builder.Services.AddControllers();
builder.Services.AddDbContext<AlbumsDbContext>(options =>
    options.UseNpgsql(connectionString));
builder.Services.AddScoped<AlbumStore>();

var app = builder.Build();

await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AlbumsDbContext>();
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
app.MapControllers();
app.Map("/api/{**path}", () => Results.Problem(
    statusCode: StatusCodes.Status404NotFound,
    title: "API endpoint not found."));
app.MapFallbackToFile("index.html");

app.Run();

public partial class Program;
