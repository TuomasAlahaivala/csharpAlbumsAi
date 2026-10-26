using AlbumsWebApp.Contracts;
using AlbumsWebApp.Models;
using AlbumsWebApp.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlbumsWebApp.Controllers;

[ApiController]
[Route("api/albums")]
public sealed class AlbumsController(AlbumStore albumStore) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<AlbumListResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AlbumListResponse>> GetAll(
        [FromQuery] string? search,
        [FromQuery] string? format,
        CancellationToken cancellationToken)
    {
        var result = await albumStore.SearchAsync(search, format, cancellationToken);
        return Ok(new AlbumListResponse(result.Albums.Select(ToResponse).ToList(), result.AvailableFormats));
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType<AlbumResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlbumResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var album = await albumStore.GetByIdAsync(id, cancellationToken);
        return album is null ? NotFound() : Ok(ToResponse(album));
    }

    [HttpPost]
    [ProducesResponseType<AlbumResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AlbumResponse>> Create(AlbumRequest request, CancellationToken cancellationToken)
    {
        var album = ToModel(request);
        await albumStore.AddAsync(album, cancellationToken);
        var response = ToResponse(album);
        return CreatedAtAction(nameof(GetById), new { id = album.Id }, response);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType<AlbumResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AlbumResponse>> Update(
        int id,
        AlbumRequest request,
        CancellationToken cancellationToken)
    {
        var album = ToModel(request);
        album.Id = id;
        return await albumStore.UpdateAsync(album, cancellationToken)
            ? Ok(ToResponse(album))
            : NotFound();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        return await albumStore.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
    }

    private static Album ToModel(AlbumRequest request) => new()
    {
        Title = request.Title.Trim(),
        Artist = request.Artist.Trim(),
        ReleaseYear = request.ReleaseYear,
        Format = request.Format.Trim(),
        Genre = request.Genre.Trim(),
        Condition = request.Condition.Trim(),
        IsFavorite = request.IsFavorite,
        Notes = request.Notes.Trim()
    };

    private static AlbumResponse ToResponse(Album album) => new(
        album.Id,
        album.Title,
        album.Artist,
        album.ReleaseYear,
        album.Format,
        album.Genre,
        album.Condition,
        album.IsFavorite,
        album.Notes);
}
