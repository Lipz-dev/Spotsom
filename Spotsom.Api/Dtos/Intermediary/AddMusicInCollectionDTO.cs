using System.ComponentModel.DataAnnotations;
using SpotSom.Api.Models;

namespace SpotSom.Api.Dtos;

public record AddMusicInCollectionDTO(
    [Required] int MusicId
);