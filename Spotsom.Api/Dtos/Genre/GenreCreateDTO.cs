using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record GenreCreateDTO(
    [Required][StringLength(15)] string Name
);
