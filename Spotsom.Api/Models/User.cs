using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Models;

/// <summary>
/// Represents an application user who can own playlists.
/// </summary>
public class User
{
    public int Id { get; set; }
    //public IFormFile? ImageCape { get; set; }
    public required string Name { get; set; }
    public DateOnly ReleaseDate { get; set; }
}