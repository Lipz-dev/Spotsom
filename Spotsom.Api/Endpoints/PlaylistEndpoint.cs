using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Data;
using SpotSom.Api.Models;
using SpotSom.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace SpotSom.Api.Endpoints;

public static class PlaylistEndpoints
{
    public static IEndpointRouteBuilder MapPlaylistEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/playlists").WithTags("Playlists");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/create", CreatedAsync);
        group.MapPut("{id:int}/edit/", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
        //const string GetPlaylistEndPoint = "GetPlaylist";
    }

    [HttpGet]
    private static async Task<Ok<List<Playlist>>> GetAllAsync(
    SpotsomContext db, CancellationToken cancellationToken) =>
    TypedResults.Ok(await db.Playlists
        .Include(p => p.User)
        .ToListAsync(cancellationToken));

    [HttpGet]
    private static async Task<Results<Ok<Playlist>, NotFound>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var playlist = await db.Playlists
            .Include(p => p.User)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
        return playlist is null ? TypedResults.NotFound() : TypedResults.Ok(playlist);
    }

    [HttpPost]
    private static async Task<Results<Created<Playlist>, NotFound>> CreatedAsync(
    CreatePlaylistDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([input.UserId], cancellationToken);
        if (user is null)
        {
            return TypedResults.NotFound();
        }

        Playlist playlist = new Playlist
        {
            Name = input.Name,
            UserId = user.Id,
            User = user,
            Musics = new List<Music>(),
            ReleaseDate = DateOnly.FromDateTime(DateTime.Now),
            Duration = new TimeOnly(00, 04, 30)
        };

        db.Playlists.Add(playlist);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/playlists/{playlist.Id}", playlist);
    }

    [HttpPut]
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

    [HttpDelete]
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

