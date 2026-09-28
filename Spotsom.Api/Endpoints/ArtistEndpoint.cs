using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Data;
using SpotSom.Api.Models;
using SpotSom.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Spotsom.Api.Endpoints;

public static class ArtistEndpoint
{
    public static IEndpointRouteBuilder MapArtistEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/artists").WithTags("Artists");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/create", CreateAsync);
        group.MapPut("/{id:int}/edit", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }


    [HttpGet]
    private static async Task<Ok<List<Artist>>> GetAllAsync(
        SpotsomContext db, CancellationToken cancellationToken) =>
        TypedResults.Ok(await db.Artists.ToListAsync(cancellationToken));

    [HttpGet]
    private static async Task<Results<Ok<Artist>, NotFound>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var artist = db.Artists.FirstOrDefault(m => m.Id == id);
        return artist is null ? TypedResults.NotFound() : TypedResults.Ok(artist);
    }

    [HttpPost]
    private static async Task<Created<Artist>> CreateAsync(
        ArtistCreateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        Artist artist = new Artist
        {
            Name = input.Name

        };

        db.Artists.Add(artist);
        await db.SaveChangesAsync(cancellationToken);

        var id = artist.GetType().GetProperty("Id")?.GetValue(artist);
        return TypedResults.Created($"/api/artists/{id}", artist);
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
