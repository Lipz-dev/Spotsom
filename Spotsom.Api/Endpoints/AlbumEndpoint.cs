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
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }

    [HttpGet]
    private static async Task<Ok<List<Album>>> GetAllAsync(
        SpotsomContext db, CancellationToken cancellationToken) =>
        TypedResults.Ok(await db.Albums
        .Include(a => a.Artist)
        .Include(mm => mm.Musics).ToListAsync(cancellationToken));

    [HttpGet]
    private static async Task<Results<Ok<Album>, NotFound>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var album = db.Albums.Include(a => a.Artist).Include(mm => mm.Musics).FirstOrDefault(m => m.Id == id);
        return album is null ? TypedResults.NotFound() : TypedResults.Ok(album);
    }

    [HttpPost]
    private static async Task<Created<Album>> CreateAsync(
        AlbumCreateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        Album album = new Album
        {
            Name = input.Name,
            ArtistId = input.ArtistId
        };

        db.Albums.Add(album);
        await db.SaveChangesAsync(cancellationToken);

        var id = album.GetType().GetProperty("Id")?.GetValue(album);
        return TypedResults.Created($"/api/albums/{id}", album);
    }


    [HttpPut]
    private static async Task<Results<Ok<Album>, NotFound>> UpdateAsync(
        int id, AlbumUpdateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var album = await db.Albums.FindAsync([id], cancellationToken);
        if (album is null)
            return TypedResults.NotFound();

        db.Entry(album).CurrentValues.SetValues(input);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(album);
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
}
