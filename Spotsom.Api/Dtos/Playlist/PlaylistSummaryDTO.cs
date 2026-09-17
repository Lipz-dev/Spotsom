
using System.ComponentModel.DataAnnotations;
//using SpotSom.Api.Models;
namespace SpotSom.Api.Dtos;


public record PlaylistDetailsDTO(
    [Required][StringLength(25)] string Name,
    [Required] IFormFile? ImageCape,
    [Required] DateOnly ReleaseDate,
    [Required] List<MusicSummaryDTO> Musics
);
