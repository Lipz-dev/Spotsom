using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record GenreResponseDTO(
    [Required][StringLength(15)] string Name,
    List<string> Musics
);
