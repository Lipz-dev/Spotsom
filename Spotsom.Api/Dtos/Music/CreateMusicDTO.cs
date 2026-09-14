using System.ComponentModel.DataAnnotations;
using SpotSom.Api.Models;

namespace SpotSom.Api.Dtos;

public record CreateMusicDTO(
    [Required][StringLength(25)] string Name,
    [Required][StringLength(25)] List<ArtistSummaryDTO> Artists,
    [Required][StringLength(15)] List<GenreSummaryDTO> Genres,
    [Required][StringLength(25)] AlbumSummaryDTO Album,
    DateOnly ReleaseDate,
    TimeOnly Duration
);