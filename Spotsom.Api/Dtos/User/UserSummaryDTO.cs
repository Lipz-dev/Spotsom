using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record UserSummaryDTO(
    [Required][StringLength(25)] string Name,
    IFormFile? ImageCape,
    DateOnly ReleaseDate
);
