
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Dtos;

public record PlaylistDTO(
    int Id,
    [Required][StringLength(25)] string Name,
    [Required] UserSummaryDTO User,
    [Required] List<MusicSummaryDTO>? Musics,
    DateOnly ReleaseDate
);
