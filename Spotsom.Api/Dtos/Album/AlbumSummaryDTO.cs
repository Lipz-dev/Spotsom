namespace SpotSom.Api.Dtos;

public record AlbumSummaryDTO(
    string name,
    IFormFile? ImageCape,
    List<ArtistSummaryDTO> Artists,
    DateOnly ReleaseDate
);
