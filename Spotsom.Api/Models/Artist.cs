using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Models;

/// <summary>
/// Represents an artist and the many-to-many links between the artist and their tracks.
/// </summary>
public class Artist
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<MusicsArtists> Musics { get; set; } = [];

    //public IFormFile? IconArtist { get; set; }
    //!make one image default for icon artist.
}