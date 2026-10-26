namespace AlbumsWebApp.Contracts;

public sealed record AlbumResponse(
    int Id,
    string Title,
    string Artist,
    int ReleaseYear,
    string Format,
    string Genre,
    string Condition,
    bool IsFavorite,
    string Notes);

public sealed record AlbumListResponse(
    IReadOnlyList<AlbumResponse> Albums,
    IReadOnlyList<string> AvailableFormats);
