using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Dtos;

public record MusicSummaryDTO(
    [Required][StringLength(30)] string Name,
    [Required] IFormFile? ImageCape,
    [Required] TimeSpan Duration,
    [Required] DateOnly ReleaseDate,
    [Required] List<ArtistSummaryDTO> Artists,
    [Required] List<GenreSummaryDTO> Genres,
    [Required] AlbumSummaryDTO Album
);
