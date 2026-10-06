
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;


public record PlaylistResponseDTO(
    [Required][StringLength(25)] string Name,
    //[Required] IFormFile? ImageCape,
    DateOnly ReleaseDate,
    [Required] List<string> Musics
);
