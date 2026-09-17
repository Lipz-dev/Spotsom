using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record GenreSummaryDTO(
    [Required][StringLength(15)] string Name
);
