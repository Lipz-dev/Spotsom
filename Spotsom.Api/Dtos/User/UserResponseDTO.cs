
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record UserResponseDTO(
    [Required][StringLength(25)] string Name,

    DateOnly ReleaseDate
);
