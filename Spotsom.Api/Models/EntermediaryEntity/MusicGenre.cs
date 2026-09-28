namespace SpotSom.Api.Models;

public class MusicGenres
{
    public int MusicId { get; set; }
    public Music? Music { get; set; }
    public required int GenreIds { get; set; }
    public Genre? Genre { get; set; }
}