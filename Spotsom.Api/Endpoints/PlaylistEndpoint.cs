using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Data;
using SpotSom.Api.Models;
using SpotSom.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SpotSom.Api.Endpoints;

public static class PlaylistEndpoints
{
    public static IEndpointRouteBuilder MapPlaylistEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/playlists").WithTags("Playlists");

        group.MapGet("/page={page:int}/size={pageSize:int}", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/create", CreatedAsync);
        group.MapPut("/{id:int}/edit/", UpdateAsync);
        group.MapPut("/{id:int}/add", AddMusicAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }

    [HttpGet]
    private static async Task<Results<Ok<PaginetedDto<PlaylistResponseDTO>>, NotFound<string>>> GetAllAsync(
    int page, int pageSize, SpotsomContext db, CancellationToken cancellationToken)
    {
        if (await db.Playlists.CountAsync(cancellationToken) <= 0)
        {
            return TypedResults.NotFound("you not create nothing playlist");
        }
        var playlists = await db.Playlists
            .OrderBy(p => p.Name)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Include(p => p.User).Include(m => m.Musics)
            .ToListAsync(cancellationToken);

        int totalItems = await db.Playlists.CountAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var response = new PaginetedDto<PlaylistResponseDTO>(
            playlists.Select(p => PlaylistResponse(p)).ToList(),
            page,
            pageSize,
            totalItems,
            totalPages
            );

        return TypedResults.Ok(response);
    }


    [HttpGet]
    private static async Task<Results<Ok<PlaylistResponseDTO>, NotFound<string>>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {

        var playlist = await db.Playlists
            .Include(p => p.User).Include(m => m.Musics)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (playlist is null)
        {
            return TypedResults.NotFound($"Playlist {id} not found.");
        }

        PlaylistResponseDTO response = PlaylistResponse(playlist);


        return TypedResults.Ok(response);
    }

    [HttpPost]
    private static async Task<Results<Created<PlaylistResponseDTO>, NotFound>> CreatedAsync(
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

        PlaylistResponseDTO response = PlaylistResponse(playlist);

        db.Playlists.Add(playlist);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Created($"/api/playlists/{playlist.Id}", response);
    }

    [HttpPut]
    private static async Task<Results<Ok<PlaylistResponseDTO>, NotFound>> UpdateAsync(
         int id, UpdatePlaylistDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var playlist = await db.Playlists.Include(m => m.Musics).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (playlist is null)
        {
            return TypedResults.NotFound();
        }

        playlist.Name = input.Name;
        await db.SaveChangesAsync(cancellationToken);

        PlaylistResponseDTO response = PlaylistResponse(playlist);

        return TypedResults.Ok(response);
    }


    [HttpPut] //ADD MUSIC IN ALBUM
    private static async Task<Results<Ok<PlaylistResponseDTO>, NotFound, BadRequest>> AddMusicAsync(
        int id, AddMusicInCollectionDTO input, SpotsomContext db,
             CancellationToken cancellationToken)
    {
        bool duplicate = false;
        var playlist = await db.Playlists.Include(m => m.Musics).FirstOrDefaultAsync(p => p.Id == id, cancellationToken);



        if (playlist == null)
        {
            return TypedResults.NotFound();
        }

        var musicAdd = await db.Musics.FindAsync([input.MusicId], cancellationToken);

        foreach (var music in playlist.Musics)
        {
            if (musicAdd?.Id == music.Id)
            {
                duplicate = true;
            }
        }

        if (musicAdd == null)
        {
            return TypedResults.NotFound();
        }

        playlist.Musics.Add(musicAdd);



        if (duplicate)
        {
            return TypedResults.BadRequest();
        }

        await db.SaveChangesAsync(cancellationToken);
        PlaylistResponseDTO response = PlaylistResponse(playlist);


        return TypedResults.Ok(response);
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

    private static PlaylistResponseDTO PlaylistResponse(Playlist playlist)
    {
        return new PlaylistResponseDTO(
            playlist.Name,
            playlist.ReleaseDate,
            playlist.Musics.Select(m => m.Name).ToList()
        );
    }
};

