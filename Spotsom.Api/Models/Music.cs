using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Models;

public class Music
{
    public int Id { get; set; }
    public required string Name { get; set; }
    //public IFormFile? ImageCape { get; set; }
    public ICollection<MusicsArtists>? Artists { get; set; } = [];
    //public required List<int> ArtistrId { get; set; }
    public ICollection<MusicGenres>? Genre { get; set; } = [];
    public Album? Album { get; set; }
    public int AlbumId { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public TimeSpan Duration { get; set; }
}