
using System.ComponentModel.DataAnnotations;
namespace SpotSom.Api.Dtos;

public record UpdatePlaylistDTO(
    [Required][StringLength(25)] string Name
);
