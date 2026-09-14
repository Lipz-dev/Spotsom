namespace SpotSom.Api.Models;

public class MusicsArtists
{
    public required int MusicId { get; set; }
    public Music? Music { get; set; }
    public required int ArtistsId { get; set; }
    public Artist? Artist { get; set; }
}