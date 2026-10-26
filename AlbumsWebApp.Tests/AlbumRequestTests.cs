using System.ComponentModel.DataAnnotations;
using AlbumsWebApp.Contracts;

namespace AlbumsWebApp.Tests;

public sealed class AlbumRequestTests
{
    [Fact]
    public void Validation_rejects_required_fields_and_out_of_range_year()
    {
        var request = new AlbumRequest { ReleaseYear = 1800, Title = "", Artist = "", Genre = "" };
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(request, new ValidationContext(request), results, true);

        Assert.False(valid);
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(AlbumRequest.ReleaseYear)));
        Assert.Contains(results, result => result.MemberNames.Contains(nameof(AlbumRequest.Title)));
    }
}
