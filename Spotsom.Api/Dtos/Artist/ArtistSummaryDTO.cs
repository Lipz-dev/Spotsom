using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record ArtistSummaryDTO(
    int Id,
    [Required][StringLength(25)] string Name,
    DateOnly ReleaseDate,
    IFormFile? ImageCape
);
