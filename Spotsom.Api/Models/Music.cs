using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Models;

/// <summary>
/// Represents a music track, its audio metadata, and its relationships to album, artists, and genres.
/// </summary>
public class Music
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? AudioPath { get; set; }
    public ICollection<MusicsArtists> Artists { get; set; } = [];
    public ICollection<MusicGenres> Genres { get; set; } = [];
    public Album? Album { get; set; }
    public int AlbumId { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public TimeSpan Duration { get; set; }
}