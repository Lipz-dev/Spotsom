using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record AlbumCreateDTO(
    [Required][StringLength(25)] string Name,
    IFormFile? ImageCape,
    [Required] List<ArtistSummaryDTO> Artists,
    DateOnly ReleaseDate
);
