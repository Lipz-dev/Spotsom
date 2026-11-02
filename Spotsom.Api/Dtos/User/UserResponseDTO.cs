
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record UserResponseDTO(
    [Required] int Id,
    [Required][StringLength(25)] string Name,
    [Required] DateOnly ReleaseDate
);
