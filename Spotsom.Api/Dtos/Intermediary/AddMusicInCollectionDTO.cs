using System.ComponentModel.DataAnnotations;

namespace SpotSom.Api.Dtos;

public record AddMusicInCollectionDTO(
    [Required] int MusicId
);