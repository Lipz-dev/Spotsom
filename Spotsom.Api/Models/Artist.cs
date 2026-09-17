using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Models;

public class Artist
{
    public int Id { get; set; }
    public required string Name { get; set; }
    //public IFormFile? IconArtist { get; set; }
    //!make one image default for icon artist.
}