using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record ArtistSummaryDTO(
    [Required][StringLength(25)] string Name,
    IFormFile? ImageCape
);
