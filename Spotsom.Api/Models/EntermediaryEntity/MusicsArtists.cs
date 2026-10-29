namespace SpotSom.Api.Models;

/// <summary>
/// Join entity that models the many-to-many relationship between tracks and artists.
/// </summary>
public class MusicsArtists
{
    public int MusicsId { get; set; }
    public Music? Music { get; set; }
    public required int ArtistsId { get; set; }
    public Artist? Artist { get; set; }
}