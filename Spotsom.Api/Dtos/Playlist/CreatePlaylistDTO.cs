
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Dtos;

public record CreatePlaylistDTO(
    [Required][StringLength(25)] string Name,
    [Required] UserSummaryDTO User,
    List<MusicSummaryDTO>? Musics,
    DateOnly ReleaseDate

);
