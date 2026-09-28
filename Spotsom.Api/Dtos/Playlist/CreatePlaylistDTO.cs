
using System.ComponentModel.DataAnnotations;
using SpotSom.Api.Models;
namespace SpotSom.Api.Dtos;

public record CreatePlaylistDTO(
    [Required][StringLength(25)] string Name,
    [Required] int UserId
);
