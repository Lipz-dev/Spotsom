using System.ComponentModel.DataAnnotations;
using SpotSom.Api.Models;

namespace SpotSom.Api.Dtos;

public record CreateMusicDTO(
    [Required][StringLength(25)] string Name,
    [Required] List<int> ArtistIds,
    [Required] List<int> GenreIds,
    [Required] int AlbumId,
    DateOnly ReleaseDate,
    TimeSpan Duration
);