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
    private static async Task<Ok<List<ArtistResponseDTO>>> GetAllAsync(
        SpotsomContext db, CancellationToken cancellationToken)
    {

        var artists = await db.Artists.Include(m => m.Musics)
                .ThenInclude(ma => ma.Music)
            .ToListAsync(cancellationToken);

        List<ArtistResponseDTO> response = artists.Select(a => ArtistResponse(a)).ToList();

        return TypedResults.Ok(response);
    }


    [HttpGet]
    private static async Task<Results<Ok<ArtistResponseDTO>, NotFound<string>>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var artist = await db.Artists.Include(m => m.Musics)
            .ThenInclude(ma => ma.Music).FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

        if (artist is null)
        {
            return TypedResults.NotFound($"artist {id} não encontrado.");
        }

        ArtistResponseDTO response = ArtistResponse(artist);

        return TypedResults.Ok(response);
    }

    [HttpPost]
    private static async Task<Created<ArtistResponseDTO>> CreateAsync(
        ArtistCreateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        Artist artist = new Artist
        {
            Name = input.Name
        };

        db.Artists.Add(artist);
        await db.SaveChangesAsync(cancellationToken);

        ArtistResponseDTO response = ArtistResponse(artist);

        var id = artist.GetType().GetProperty("Id")?.GetValue(artist);
        return TypedResults.Created($"/api/artists/{id}", response);
    }

    [HttpPut]
    private static async Task<Results<Ok<ArtistResponseDTO>, NotFound>> UpdateAsync(
        int id, ArtistUpdateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var artist = await db.Artists.Include(m => m.Musics)
            .ThenInclude(ma => ma.Music).FirstOrDefaultAsync(art => art.Id == id, cancellationToken);
        if (artist is null)
            return TypedResults.NotFound();

        artist.Name = input.Name;
        await db.SaveChangesAsync(cancellationToken);

        ArtistResponseDTO response = ArtistResponse(artist);

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

    private static ArtistResponseDTO ArtistResponse(Artist artist)
    {
        var musics = artist.Musics.Select(m => m.Music).ToList();



        return new ArtistResponseDTO(artist.Name,
            musics.Select(m => m.Name).ToList()
            );
    }
}
