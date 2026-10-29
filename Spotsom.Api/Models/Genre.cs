namespace SpotSom.Api.Models;

/// <summary>
/// Represents a musical genre and its many-to-many links to tracks.
/// </summary>
public class Genre
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<MusicGenres> Musics { get; set; } = [];
}