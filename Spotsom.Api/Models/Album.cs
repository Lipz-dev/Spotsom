using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Models;

/// <summary>
/// Represents an album owned by one artist and containing the tracks released in that collection.
/// </summary>
public class Album
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<Music> Musics { get; set; } = new List<Music>();
    public Artist? Artist { get; set; }
    public int ArtistId { get; set; }
    public DateOnly ReleaseDate { get; set; }
}