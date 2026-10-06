
using System.ComponentModel.DataAnnotations;
using SpotSom.Api.Models;
namespace SpotSom.Api.Dtos;

public record UpdateMusicDTO(
    [StringLength(30)] string Name//,
                                  //IFormFile CapeImage
);
