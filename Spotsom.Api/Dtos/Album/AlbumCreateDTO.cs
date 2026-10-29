using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record AlbumCreateDTO(
    [Required][StringLength(25)] string Name,
    [Required] int ArtistId

);
