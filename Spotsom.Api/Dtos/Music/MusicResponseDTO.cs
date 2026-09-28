using Microsoft.AspNetCore.Http;
using SpotSom.Api.Models;
using System.ComponentModel.DataAnnotations;


namespace SpotSom.Api.Dtos;

public record MusicResponseDTO(
    [Required][StringLength(30)] string Name,

    [Required] TimeSpan Duration,
    [Required] DateOnly ReleaseDate,
    [Required] List<string> Artists,
    [Required] List<string> Genres,
    [Required] string Album
);
