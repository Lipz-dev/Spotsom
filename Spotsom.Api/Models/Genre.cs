namespace SpotSom.Api.Models;

public class Genre
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public ICollection<MusicGenres>? Musics { get; set; } = [];
}