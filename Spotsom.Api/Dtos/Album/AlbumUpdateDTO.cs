using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record AlbumUpdateDTO(
    [Required][StringLength(25)] string Name//,
                                            //IFormFile? ImageCape
);
