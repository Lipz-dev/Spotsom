using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Data;
using SpotSom.Api.Models;
using SpotSom.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Spotsom.Api.Endpoints;

public static class AlbumEndpoint
{
    public static IEndpointRouteBuilder MapAlbumEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/albums").WithTags("Albums");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);

        group.MapPost("/create", CreateAsync);

        group.MapPut("/{id:int}/edit", UpdateAsync);
        group.MapPut("/{id:int}/addmusic", AddMusicAsync);

        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }

    [HttpGet]
    private static async Task<Ok<List<AlbumResponseDTO>>> GetAllAsync(
        SpotsomContext db, CancellationToken cancellationToken)
    {
        var albums = await db.Albums.Include(a => a.Artist)
        .Include(m => m.Musics).ToListAsync(cancellationToken);

        List<AlbumResponseDTO> albumResponseDTO = albums.Select(al => new AlbumResponseDTO(
            al.Name,
            al.Musics.Select(m => m.Name).ToList(),
            al.Artist.Name,
            al.ReleaseDate)
        ).ToList();
        return TypedResults.Ok(albumResponseDTO);
    }

    [HttpGet]
    private static async Task<Results<Ok<AlbumResponseDTO>, NotFound>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var album = db.Albums.Include(a => a.Artist)
        .Include(m => m.Musics).FirstOrDefault(m => m.Id == id);

        AlbumResponseDTO response = new AlbumResponseDTO(
            album.Name,
            album.Musics.Select(m => m.Name).ToList(),
            album.Artist.Name,
            album.ReleaseDate
        );
        return album is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    [HttpPost]
    private static async Task<Results<Created<AlbumResponseDTO>, BadRequest>> CreateAsync(
        AlbumCreateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {

        Album album = new Album
        {
            Name = input.Name,
            Artist = await db.Artists.FindAsync([input.ArtistId], cancellationToken),
            ArtistId = input.ArtistId,
            ReleaseDate = input.ReleaseDate
        };


        //! verification test
        var artistAlbum = await db.Artists.FindAsync([album.ArtistId], cancellationToken);

        if (artistAlbum == null)
        {
            TypedResults.BadRequest("invalid input! artist is null");
        }

        db.Albums.Add(album);
        await db.SaveChangesAsync(cancellationToken);


        AlbumResponseDTO albumResponseDTO = new(
            album.Name,
            [],
            album.Artist.Name,
            album.ReleaseDate);

        int id = album.Id;
        return TypedResults.Created($"/api/albums/{id}", albumResponseDTO);
    }


    [HttpPut]
    private static async Task<Results<Ok<AlbumResponseDTO>, NotFound>> UpdateAsync(
        int id, AlbumUpdateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var album = await db.Albums.Include(m => m.Musics).Include(a => a.Artist).FirstOrDefaultAsync(al => al.Id == id, cancellationToken);

        if (album is null)
        {
            return TypedResults.NotFound();
        }

        album.Name = input.Name;

        await db.SaveChangesAsync(cancellationToken);
        AlbumResponseDTO response = new(
            album.Name,
            album.Musics.Select(m => m.Name).ToList(),
            album.Artist.Name,
            album.ReleaseDate
        );
        return TypedResults.Ok(response);

    }

    [HttpPut] //ADD MUSIC IN ALBUM
    private static async Task<Results<Ok<AlbumResponseDTO>, NotFound, BadRequest>> AddMusicAsync(
        int id, AddMusicInCollectionDTO input, SpotsomContext db
        , CancellationToken cancellationToken)
    {
        bool duplicate = false;
        var album = await db.Albums.Include(a => a.Artist).ThenInclude(am => am.Musics)
            .Include(m => m.Musics).FirstOrDefaultAsync(al => al.Id == id, cancellationToken);

        var musicAdd = await db.Musics.FindAsync([input.MusicId], cancellationToken);



        foreach (var music in album.Musics)
        {
            if (musicAdd.Id == music.Id)
            {
                duplicate = true;
            }
        }


        if (album == null || musicAdd == null)
        {
            TypedResults.BadRequest("album or music or artistAl not exist");
        }

        album.Musics.Add(musicAdd);

        AlbumResponseDTO response = new(
        album.Name,
        album.Musics.Select(m => m.Name).ToList(),
        album.Artist.Name,
        album.ReleaseDate
        );

        if (duplicate)
        {
            return TypedResults.BadRequest();
        }

        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.Ok(response);
    }


    [HttpDelete]
    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var album = await db.Albums.FindAsync([id], cancellationToken);
        if (album is null)
            return TypedResults.NotFound();

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
