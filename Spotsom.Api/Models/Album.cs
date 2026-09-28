using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Models;

public class Album
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Music>? Musics { get; set; } = new List<Music>();
    //public IFormFile? ImageCape { get; set; }
    public Artist? Artist { get; set; }
    public required int ArtistId { get; set; }
    public DateOnly ReleaseDate { get; set; }
}