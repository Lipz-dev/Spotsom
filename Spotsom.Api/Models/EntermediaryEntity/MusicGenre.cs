namespace SpotSom.Api.Models;

public class MusicGenre
{
    public required int MusicId { get; set; }
    public Music? Music { get; set; }
    public required int GenreId { get; set; }
    public Genre? Genre { get; set; }
}