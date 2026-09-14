using SpotSom.Api.Dtos;
using SpotSom.Api.Data;

namespace SpotSom.Api.Endpoints;

public static class PlaylistEndpoints
{
    public static void MapPlaylistEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/playlists");
        const string GetMusicEndPoint = "GetPlaylist";




        // GET /Musics/
        group.MapGet("/", () =>
        {
            return DataAnnotations.playlists is null ? Results.NotFound() : Results.Ok(DataAnnotations.playlists);
        });

        // GET /Musics/1
        group.MapGet("/{id}", (int id) =>
        {
            var playlist = DataAnnotations.playlists.Find(music => music.Id == id);
            return playlist is null ? Results.NotFound() : Results.Ok(playlist);

        }).WithName(GetMusicEndPoint);

        // POST /Musics/
        group.MapPost("/", (CreatePlaylistDTO createPlaylistDTO) =>
        {


            PlaylistDTO playlistDto =
                new
                (DataAnnotations.playlists.Count + 1,
                createPlaylistDTO.Name,
                createPlaylistDTO.Artist,
                new List<MusicDTO>(),
                createPlaylistDTO.ReleaseDate
                );

            DataAnnotations.playlists.Add(playlistDto);

            return Results.CreatedAtRoute(GetMusicEndPoint, new { id = playlistDto.Id }, playlistDto);
        });

        group.MapPut("/{id}", (int id, UpdatePlaylistDTO updatePlaylistDTO) =>
        {
            var index = DataAnnotations.playlists.FindIndex(playlist => playlist.Id == id);
            if (index == -1)
            {
                return Results.NotFound();
            }

            DataAnnotations.playlists[index] = new(
                index,
                updatePlaylistDTO.Name,
                DataAnnotations.playlists[index].Artist,
                DataAnnotations.playlists[index].Musics,
                DataAnnotations.playlists[index].ReleaseDate
            );
            return Results.NoContent();
        }
        );


        group.MapPut("/{idPlaylist}/add/{idMusic}", (int idPlaylist, int idMusic) =>
        {
            var playlistIndex = DataAnnotations.playlists.FindIndex(playlist => playlist.Id == idPlaylist);

            if (playlistIndex == -1)
            {
                return Results.NotFound();
            }
            var music = DataAnnotations.musics.Find(music => music.Id == idMusic);

            if (music is MusicDTO)
            {
                DataAnnotations.playlists[idPlaylist - 1].Musics?.Add(music);
            }
            else
            {
                return Results.NotFound();
            }

            return Results.NoContent();

        });

        group.MapDelete("/{id}", (int id) =>
        {
            var index = DataAnnotations.playlists.FindIndex(playlist => playlist.Id == id);
            if (index == -1)
            {
                return Results.NotFound();
            }

            DataAnnotations.playlists.Remove(DataAnnotations.playlists[index]);
            return Results.NoContent();
        });








        //     return Results.NoContent();

        // });

        // group.MapDelete("/{id}", (int id) =>
        // {
        //     playlists.RemoveAll(music => music.Id == id);

        //     return Results.NoContent();
        // });
    }

}
