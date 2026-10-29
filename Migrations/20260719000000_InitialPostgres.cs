using AlbumsWebApp.Data;
using AlbumsWebApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlbumsWebApp.Migrations;

[DbContext(typeof(AlbumsDbContext))]
[Migration("20260719000000_InitialPostgres")]
public sealed class InitialPostgres : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Albums",
            columns: table => new
            {
                Id = table.Column<int>(type: "integer", nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                Title = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                Artist = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                ReleaseYear = table.Column<int>(type: "integer", nullable: false),
                Format = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Genre = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                Condition = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                IsFavorite = table.Column<bool>(type: "boolean", nullable: false),
                Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Albums", x => x.Id);
            });

        migrationBuilder.InsertData(
            table: "Albums",
            columns: new[] { "Id", "Artist", "Condition", "Format", "Genre", "IsFavorite", "Notes", "ReleaseYear", "Title" },
            values: new object[,]
            {
                { 1, "Miles Davis", "Very good", "Vinyl", "Jazz", true, "Classic modal jazz pressing.", 1959, "Kind of Blue" },
                { 2, "Kate Bush", "Good", "CD", "Art pop", false, "Keep near the 80s shelf.", 1985, "Hounds of Love" }
            });

        migrationBuilder.Sql("SELECT setval(pg_get_serial_sequence('\"Albums\"', 'Id'), 2, true);");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Albums");
    }

    protected override void BuildTargetModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.8")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity<Album>(entity =>
        {
            entity.Property<int>(nameof(Album.Id))
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            entity.Property<string>(nameof(Album.Artist))
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            entity.Property<string>(nameof(Album.Condition))
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            entity.Property<string>(nameof(Album.Format))
                .IsRequired()
                .HasMaxLength(80)
                .HasColumnType("character varying(80)");

            entity.Property<string>(nameof(Album.Genre))
                .IsRequired()
                .HasMaxLength(80)
                .HasColumnType("character varying(80)");

            entity.Property<bool>(nameof(Album.IsFavorite))
                .HasColumnType("boolean");

            entity.Property<string>(nameof(Album.Notes))
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("character varying(500)");

            entity.Property<int>(nameof(Album.ReleaseYear))
                .HasColumnType("integer");

            entity.Property<string>(nameof(Album.Title))
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            entity.HasKey(nameof(Album.Id));
            entity.ToTable("Albums");

            entity.HasData(
                new Album
                {
                    Id = 1,
                    Artist = "Miles Davis",
                    Condition = "Very good",
                    Format = "Vinyl",
                    Genre = "Jazz",
                    IsFavorite = true,
                    Notes = "Classic modal jazz pressing.",
                    ReleaseYear = 1959,
                    Title = "Kind of Blue"
                },
                new Album
                {
                    Id = 2,
                    Artist = "Kate Bush",
                    Condition = "Good",
                    Format = "CD",
                    Genre = "Art pop",
                    IsFavorite = false,
                    Notes = "Keep near the 80s shelf.",
                    ReleaseYear = 1985,
                    Title = "Hounds of Love"
                });
        });
#pragma warning restore 612, 618
    }
}
