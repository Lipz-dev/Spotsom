namespace SpotSom.Api.Models;

/// <summary>
/// Join entity that models the many-to-many relationship between tracks and genres.
/// </summary>
public class MusicGenres
{
    public int MusicsId { get; set; }
    public Music? Music { get; set; }
    public required int GenresId { get; set; }
    public Genre? Genre { get; set; }
}