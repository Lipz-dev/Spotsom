using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Models;

public class Music
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public IFormFile? ImageCape { get; set; }
    public List<MusicsArtists>? Artists { get; set; }
    public required int AuthorId { get; set; }
    public List<Genre>? Genre { get; set; }
    public required int GenreId { get; set; }
    public Album? Album { get; set; }
    public required int AlbumId { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public TimeOnly Duration { get; set; }
}