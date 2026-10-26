using AlbumsWebApp.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
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
}
