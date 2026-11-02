
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;


public record PlaylistResponseDTO(
    [Required] int Id,
    [Required][StringLength(25)] string Name,

    [Required] DateOnly ReleaseDate,
    [Required] List<string> Musics
);
