
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Dtos;

public record UpdateMusicDTO(
    [StringLength(30)] string Name,
    int AlbumId,
    List<int> GenreIds,
    List<int> ArtistIds
//IFormFile CapeImage
);
