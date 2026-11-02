using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record GenreResponseDTO(
    [Required] int Id,
    [Required][StringLength(15)] string Name,
    [Required] List<string> Musics
);
