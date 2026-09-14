using SpotSom.Api.Dtos;

namespace SpotSom.Api.Data;

public static class DataAnnotations
{

    public static List<MusicSummaryDTO> musics = [
                new(
                    1,
                    "Beat it",
                    "Michael Jackson",
                    "Pop",
                    "Beat it",
                    new DateOnly(1970, 9, 2),
                    new TimeOnly(00, 2, 10)),

                new(
                    2,
                    "Boheiam Rhapsody",
                    "Queen",
                    "Pop Rock",
                    "A Night At The Opera",
                    new DateOnly(1980, 10,24),
                    new TimeOnly(00, 5,55)
                )
                ];

    public static List<PlaylistDTO> playlists = [
            new(
                    1,
                    "Sinta o Som",
                    "Felipe",
                    new List<MusicSummaryDTO>(),
                    new DateOnly(2026, 8,31)
                    ),


                new(
                    2, "Sinta o Som 2",
                    "Hellô",
                    new List<MusicSummaryDTO>(),
                    new DateOnly(2026, 8,31)
                    ),

                new(
                    3, "meu pau",
                    "eu",
                    new List<MusicSummaryDTO>(),
                    new DateOnly(2026, 8,20)
                    )
                ];
}
