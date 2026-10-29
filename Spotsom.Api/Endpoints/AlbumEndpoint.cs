using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Data;
using SpotSom.Api.Models;
using SpotSom.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace Spotsom.Api.Endpoints;

/// <summary>
/// Registers album routes and their CRUD and music-association operations.
/// </summary>
public static class AlbumEndpoint
{
    /// <summary>
    /// Maps all album endpoints under the <c>/api/albums</c> route group.
    /// </summary>
    public static IEndpointRouteBuilder MapAlbumEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/albums").WithTags("Albums");

        group.MapGet("/page={page:int}/size={pageSize:int}", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);

        group.MapPost("/create", CreateAsync);

        group.MapPut("/{id:int}/edit", UpdateAsync);
        group.MapPut("/{id:int}/addmusic", AddMusicAsync);

        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }

    [HttpGet]
    private static async Task<Results<Ok<PaginetedDto<AlbumResponseDTO>>, NotFound<string>, BadRequest<string>>> GetAllAsync(
        int page, int pageSize, SpotsomContext db, CancellationToken cancellationToken)
    {

        if (page <= 0 || pageSize <= 0)
        {
            return TypedResults.BadRequest("Page and pageSize must be greater than zero.");
        }
        var albums = await db.Albums.OrderBy(a => a.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize).Include(a => a.Artist)
            .Include(m => m.Musics).ToListAsync(cancellationToken);

        int totalItems = await db.Albums.CountAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        if (page > totalPages)
        {
            return TypedResults.NotFound($"Page {page} no exist.");
        }

        var response = new PaginetedDto<AlbumResponseDTO>(
            albums.Select(m => AlbumResponse(m)).ToList(),
            page,
            pageSize,
            totalItems,
            totalPages
        );

        return TypedResults.Ok(response);
    }

    [HttpGet]
    private static async Task<Results<Ok<AlbumResponseDTO>, NotFound<string>>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var album = await db.Albums.Include(a => a.Artist)
        .Include(m => m.Musics).FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (album is null)
        {
            return TypedResults.NotFound($"Album {id} not found.");
        }

        AlbumResponseDTO response = AlbumResponse(album);
        return TypedResults.Ok(response);
    }

    [HttpPost]
    private static async Task<Results<Created<AlbumResponseDTO>, NotFound<string>, BadRequest<string>>> CreateAsync(
        AlbumCreateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var artistsAl = await db.Artists.FindAsync([input.ArtistId], cancellationToken);

        if (artistsAl is null)
        {
            return TypedResults.NotFound($"Artist {input.ArtistId} for the album was not found.");
        }

        Album album = new Album
        {
            Name = input.Name,
            Artist = artistsAl,
            ArtistId = input.ArtistId,
            ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        if (album is null)
        {
            return TypedResults.BadRequest("Incorrect entry for album creation.");
        }

        var artistAlbum = await db.Artists.FindAsync([album.ArtistId], cancellationToken);

        if (artistAlbum == null)
        {
            return TypedResults.BadRequest("invalid input! artist is null.");
        }

        if (album is not null)
        {
            var albumTeste = await db.Users.Select(m => m.Name.ToLower()).ToListAsync(cancellationToken);
            var albumFilter = albumTeste.Where(m => m == input.Name.ToLower());

            if (albumFilter.Count() > 0)
            {
                return TypedResults.BadRequest($"User {input.Name} already exists.");
            }
        }
        else
        {
            return TypedResults.BadRequest("Invalid input! album is null.");
        }

        db.Albums.Add(album);

        await db.SaveChangesAsync(cancellationToken);


        AlbumResponseDTO albumResponseDTO = AlbumResponse(album);

        int id = album.Id;
        return TypedResults.Created($"/api/albums/{id}", albumResponseDTO);
    }


    [HttpPut]
    private static async Task<Results<Ok<AlbumResponseDTO>, NotFound<string>>> UpdateAsync(
        int id, AlbumUpdateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var album = await db.Albums.Include(m => m.Musics)
        .Include(a => a.Artist).FirstOrDefaultAsync(al => al.Id == id, cancellationToken);

        if (album is null)
        {
            return TypedResults.NotFound($"Album {id} not found.");
        }

        album.Name = input.Name;

        await db.SaveChangesAsync(cancellationToken);
        AlbumResponseDTO response = AlbumResponse(album);
        return TypedResults.Ok(response);
    }

    // Associates a track with the album; duplicate associations are rejected.
    [HttpPut]
    private static async Task<Results<Ok<AlbumResponseDTO>, NotFound<string>, BadRequest<string>>> AddMusicAsync(
        int id, AddMusicInCollectionDTO input, SpotsomContext db
        , CancellationToken cancellationToken)
    {
        bool duplicate = false;
        var album = await db.Albums.Include(a => a.Artist)
            .Include(m => m.Musics).ThenInclude(ma => ma.Artists)
            .FirstOrDefaultAsync(al => al.Id == id, cancellationToken);

        var musicAdd = await db.Musics.FindAsync([input.MusicId], cancellationToken);

        if (album is null || musicAdd is null)
        {
            return TypedResults.NotFound("album or musicAdd not found.");
        }

        foreach (var music in album.Musics)
        {
            if (musicAdd.Id == music.Id)
            {
                duplicate = true;
            }
        }

        if (duplicate)
        {
            return TypedResults.BadRequest("This music is already in the album; choose another one.");
        }

        album.Musics.Add(musicAdd);

        AlbumResponseDTO response = AlbumResponse(album);

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(response);
    }


    [HttpDelete]
    private static async Task<Results<NoContent, NotFound<string>>> DeleteAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var album = await db.Albums.FindAsync([id], cancellationToken);
        if (album is null)
        {
            return TypedResults.NotFound($"Album {id} not found.");
        }

        db.Albums.Remove(album);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static AlbumResponseDTO AlbumResponse(Album album)
    {
        return new AlbumResponseDTO(
        album.Name,
        album.Musics.Select(m => m.Name).ToList(),
        album.Artist.Name,
        album.ReleaseDate);
    }
}
