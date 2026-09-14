using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Dtos;

public record MusicSummaryDTO(
    int Id,
    [Required][StringLength(25)] string Name,
    [Required] IFormFile ImageCape,
    [Required] List<ArtistSummaryDTO> Artists,
    [Required] List<GenreSummaryDTO> Genres,
    [Required] AlbumSummaryDTO Album,
    DateOnly ReleaseDate,
    TimeOnly Duration
);
