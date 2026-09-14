// using SpotSom.Api.Dtos;
// using SpotSom.Api.Data;
// using Microsoft.EntityFrameworkCore;
// using SpotSom.Api.Models;

// namespace SpotSom.Api.Endpoints
// {
//     public static class MusicEndpoints
//     {
//         public static void MapMusicEndpoints(this WebApplication app)
//         {
//             var group = app.MapGroup("/musics");
//             const string GetMusicEndPoint = "GetMusic";




//             // GET /Musics/
//             group.MapGet("/", async (SpotsomContext dbContext) => await dbContext.Musics.Select(music => new MusicSummaryDTO
//             {

//             }));


//             // {
//             //     return musics is null ? Results.NotFound() : Results.Ok(musics);
//             // });

//             // GET /Musics/1
//             group.MapGet("/{id}", (int id) =>
//             {
//                 var music = musics.Find(music => music.Id == id);
//                 return music is null ? Results.NotFound() : Results.Ok(music);

//             }).WithName(GetMusicEndPoint);

//             // POST /Musics/
//             group.MapPost("/", (CreateMusicDTO createMusicDTO) =>
//             {
//                 MusicDTO musicDto =
//                     new
//                     (musics.Count + 1,
//                     createMusicDTO.Name,
//                     createMusicDTO.Artist,
//                     createMusicDTO.Genre,
//                     createMusicDTO.Album,
//                     createMusicDTO.ReleaseDate,
//                     createMusicDTO.Duration);

//                 musics.Add(musicDto);

//                 return Results.CreatedAtRoute(GetMusicEndPoint, new { id = musicDto.Id }, musicDto);
//             });

//             group.MapPut("/{id}", (int id, UpdateMusicDTO updateMusicDTO) =>
//             {
//                 var index = musics.FindIndex(music => music.Id == id);
//                 if (index == -1)
//                 {
//                     return Results.NotFound();
//                 }

//                 musics[index] = new(
//                 index,
//                 updateMusicDTO.Name,
//                 updateMusicDTO.Artist,
//                 updateMusicDTO.Genre,
//                 updateMusicDTO.Album,
//                 musics[index].ReleaseDate,
//                 musics[index].Duration
//             );

//                 return Results.NoContent();

//             });

//             group.MapDelete("/{id}", (int id) =>
//             {
//                 musics.RemoveAll(music => music.Id == id);

//                 return Results.NoContent();
//             });
//         }

//     }
// }