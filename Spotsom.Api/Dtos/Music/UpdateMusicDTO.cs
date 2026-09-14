
using System.ComponentModel.DataAnnotations;
using SpotSom.Api.Models;
namespace SpotSom.Api.Dtos;

public record UpdateMusicDTO(
    [Required][StringLength(25)] string Name,
    [Required][StringLength(25)] List<ArtistSummaryDTO> Artists,
    [Required][StringLength(15)] List<GenreSummaryDTO> Genre,
    [Required][StringLength(25)] AlbumSummaryDTO Album
);
