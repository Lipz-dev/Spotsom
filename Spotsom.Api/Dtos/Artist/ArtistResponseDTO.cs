
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record ArtistResponseDTO(
    [Required][StringLength(25)] string Name,
    List<string> Musics
//IFormFile? ImageCape
);
