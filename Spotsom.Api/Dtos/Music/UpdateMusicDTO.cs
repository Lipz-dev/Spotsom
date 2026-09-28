
using System.ComponentModel.DataAnnotations;
using SpotSom.Api.Models;
namespace SpotSom.Api.Dtos;

public record UpdateMusicDTO(
    [StringLength(25)] string Name//,
                                  //IFormFile CapeImage
);
