using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record GenreUpdateDTO(
    [Required][StringLength(15)] string Name
);
