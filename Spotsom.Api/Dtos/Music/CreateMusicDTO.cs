using System.ComponentModel.DataAnnotations;


namespace SpotSom.Api.Dtos;

public record CreateMusicDTO(
    [Required] IFormFile Audio
);