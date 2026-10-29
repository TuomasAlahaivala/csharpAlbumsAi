using AlbumsWebApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AlbumsWebApp.Tests;

public sealed class InitialMigrationTests
{
    [Fact]
    public void Initial_migration_generates_schema_and_seed_data_script()
    {
        var options = new DbContextOptionsBuilder<AlbumsDbContext>()
            .UseNpgsql("Host=localhost;Database=albums;Username=albums;Password=albums_dev")
            .Options;
        using var context = new AlbumsDbContext(options);

        var script = context.GetService<IMigrator>().GenerateScript();

        Assert.Contains("CREATE TABLE \"Albums\"", script);
        Assert.Contains("Kind of Blue", script);
        Assert.Contains("Hounds of Love", script);
    }
}
