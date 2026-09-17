using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Models;

public class Playlist
{
    public int Id { get; set; }
    //public IFormFile? ImageCape { get; set; }
    public required string Name { get; set; }
    public User? User { get; set; }
    public required int UserId { get; set; }
    public List<Music>? Musics { get; set; }
    public DateOnly ReleaseDate { get; set; }
    public TimeOnly Duration { get; set; }
}