using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record GenreSummaryDTO(
    int Id,
    [Required][StringLength(15)] string Name
);
