using AlbumsWebApp.Data;
using AlbumsWebApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AlbumsWebApp.Services;

public sealed class AlbumStore
{
    private readonly AlbumsDbContext _dbContext;

    public AlbumStore(AlbumsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<(IReadOnlyList<Album> Albums, IReadOnlyList<string> AvailableFormats)> SearchAsync(
        string? search,
        string? format,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Albums.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalizedSearch = search.Trim().ToLower();
            query = query.Where(album =>
                album.Title.ToLower().Contains(normalizedSearch) ||
                album.Artist.ToLower().Contains(normalizedSearch) ||
                album.Genre.ToLower().Contains(normalizedSearch));
        }

        if (!string.IsNullOrWhiteSpace(format))
        {
            var normalizedFormat = format.Trim().ToLower();
            query = query.Where(album => album.Format.ToLower() == normalizedFormat);
        }

        var albums = await query
            .OrderByDescending(album => album.IsFavorite)
            .ThenBy(album => album.Artist)
            .ThenBy(album => album.ReleaseYear)
            .ToListAsync(cancellationToken);

        var storedFormats = await _dbContext.Albums
            .AsNoTracking()
            .Select(album => album.Format)
            .Where(value => value != "")
            .ToListAsync(cancellationToken);

        var availableFormats = storedFormats
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToList();

        return (albums, availableFormats);
    }

    public async Task<Album?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Albums
            .AsNoTracking()
            .SingleOrDefaultAsync(album => album.Id == id, cancellationToken);
    }

    public async Task AddAsync(Album album, CancellationToken cancellationToken = default)
    {
        _dbContext.Albums.Add(album);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UpdateAsync(Album album, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Albums.SingleOrDefaultAsync(
            item => item.Id == album.Id,
            cancellationToken);
        if (existing is null)
        {
            return false;
        }

        existing.Title = album.Title;
        existing.Artist = album.Artist;
        existing.ReleaseYear = album.ReleaseYear;
        existing.Format = album.Format;
        existing.Genre = album.Genre;
        existing.Condition = album.Condition;
        existing.IsFavorite = album.IsFavorite;
        existing.Notes = album.Notes;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var album = await _dbContext.Albums.SingleOrDefaultAsync(
            item => item.Id == id,
            cancellationToken);
        if (album is null)
        {
            return false;
        }

        _dbContext.Albums.Remove(album);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
