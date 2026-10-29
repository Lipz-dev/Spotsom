
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record AlbumUpdateDTO(
    [Required][StringLength(30)] string Name//,
                                            //IFormFile? ImageCape
);
