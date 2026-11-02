

using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record AlbumResponseDTO(
    [Required] int Id,
    [Required][StringLength(30)] string Name,
    [Required] List<string> Musics,
    [Required] string Artist,
    [Required] DateOnly ReleaseDate
);
