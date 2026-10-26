using System.ComponentModel.DataAnnotations;

namespace AlbumsWebApp.Contracts;

public sealed class AlbumRequest
{
    [Required, MinLength(1), StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [Required, MinLength(1), StringLength(120)]
    public string Artist { get; set; } = string.Empty;

    [Range(1888, 2100)]
    public int ReleaseYear { get; set; } = DateTime.UtcNow.Year;

    [Required, StringLength(80)]
    public string Format { get; set; } = "Vinyl";

    [Required, StringLength(80)]
    public string Genre { get; set; } = string.Empty;

    [Required, StringLength(120)]
    public string Condition { get; set; } = "Good";

    public bool IsFavorite { get; set; }

    [StringLength(500)]
    public string Notes { get; set; } = string.Empty;
}
