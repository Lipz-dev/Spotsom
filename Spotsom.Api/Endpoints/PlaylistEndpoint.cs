using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Data;
using SpotSom.Api.Models;
using SpotSom.Api.Dtos;

namespace SpotSom.Api.Endpoints;

public static class PlaylistEndpoints
{
    public static void MapPlaylistEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/playlists").WithTags("Playlists");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/{idUser}/create", CreatedAsync);
        group.MapPut("{id:int}/edit/", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        //const string GetPlaylistEndPoint = "GetPlaylist";
    }


    private static async Task<Ok<List<Playlist>>> GetAllAsync(
    SpotsomContext db, CancellationToken cancellationToken) =>
    TypedResults.Ok(await db.Playlists.ToListAsync(cancellationToken));


    private static async Task<Results<Ok<Playlist>, NotFound>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var playlist = await db.Playlists.FindAsync([id], cancellationToken);
        return playlist is null ? TypedResults.NotFound() : TypedResults.Ok(playlist);
    }


    private static async Task<Created<Playlist>> CreatedAsync(
        int idUser,
    CreatePlaylistDTO playlistDto, SpotsomContext db, CancellationToken cancellationToken)
    {

        Playlist playlist = new Playlist
        {
            Name = playlistDto.Name,
            User = await db.Users.FindAsync([idUser]),
            UserId = idUser,
            Musics = new List<Music>(),
            ReleaseDate = DateOnly.FromDateTime(DateTime.Now),
            Duration = new TimeOnly(00, 04, 30)
        };

        db.Playlists.Add(playlist);
        await db.SaveChangesAsync(cancellationToken);

        var id = playlist.GetType().GetProperty("Id")?.GetValue(playlist);
        return TypedResults.Created($"/api/playlists/{id}", playlist);
    }


    private static async Task<Results<Ok<Playlist>, NotFound>> UpdateAsync(
         int id, UpdatePlaylistDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var playlist = await db.Playlists.FindAsync([id], cancellationToken);
        if (playlist is null)
        {
            return TypedResults.NotFound();
        }

        db.Entry(playlist).CurrentValues.SetValues(input);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(playlist);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var playlist = await db.Playlists.FindAsync([id], cancellationToken);
        if (playlist is null)
        {
            return TypedResults.NotFound();
        }

        db.Playlists.Remove(playlist);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }
};

