using AlbumsWebApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlbumsWebApp.Migrations;

[DbContext(typeof(AlbumsDbContext))]
public sealed class AlbumsDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.8")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("AlbumsWebApp.Models.Album", entity =>
        {
            entity.Property<int>("Id")
                .ValueGeneratedOnAdd()
                .HasColumnType("integer")
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            entity.Property<string>("Artist")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            entity.Property<string>("Condition")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            entity.Property<string>("Format")
                .IsRequired()
                .HasMaxLength(80)
                .HasColumnType("character varying(80)");

            entity.Property<string>("Genre")
                .IsRequired()
                .HasMaxLength(80)
                .HasColumnType("character varying(80)");

            entity.Property<bool>("IsFavorite")
                .HasColumnType("boolean");

            entity.Property<string>("Notes")
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("character varying(500)");

            entity.Property<int>("ReleaseYear")
                .HasColumnType("integer");

            entity.Property<string>("Title")
                .IsRequired()
                .HasMaxLength(120)
                .HasColumnType("character varying(120)");

            entity.HasKey("Id");
            entity.ToTable("Albums");

            entity.HasData(
                new
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
                new
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
