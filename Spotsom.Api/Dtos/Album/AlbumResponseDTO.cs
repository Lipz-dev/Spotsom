using SpotSom.Api.Models;

namespace SpotSom.Api.Dtos;

public record AlbumResponseDTO(
    string name,
    //IFormFile? ImageCape,
    List<string> Musics,
    string Artist,
    DateOnly ReleaseDate
);
