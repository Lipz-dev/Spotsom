
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record ArtistResponseDTO(
    [Required] int Id,
    [Required][StringLength(25)] string Name,
    [Required] List<string> Musics
);
