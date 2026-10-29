using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Data;
using SpotSom.Api.Models;
using SpotSom.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Spotsom.Api.Endpoints;

/// <summary>
/// Registers artist routes and exposes their associated music catalog.
/// </summary>
public static class ArtistEndpoint
{
    /// <summary>
    /// Maps all artist endpoints under the <c>/api/artists</c> route group.
    /// </summary>
    public static IEndpointRouteBuilder MapArtistEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/artists").WithTags("Artists");

        group.MapGet("/page={page:int}/size={pageSize:int}", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/create", CreateAsync);
        group.MapPut("/{id:int}/edit", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }


    [HttpGet]
    private static async Task<Results<Ok<PaginetedDto<ArtistResponseDTO>>, BadRequest<string>>> GetAllAsync(
        int page, int pageSize,
        SpotsomContext db, CancellationToken cancellationToken)
    {

        if (page <= 0 || pageSize <= 0)
        {
            return TypedResults.BadRequest("Page and pageSize must be greater than zero.");
        }

        var artists = await db.Artists.OrderBy(m => m.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Musics)
                .ThenInclude(ma => ma.Music)
            .ToListAsync(cancellationToken);

        int totalItems = await db.Artists.CountAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var response = new PaginetedDto<ArtistResponseDTO>(
            artists.Select(m => ArtistResponse(m)).ToList(),
            page,
            pageSize,
            totalItems,
            totalPages
        );

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
            return TypedResults.NotFound($"Artist {id} not found.");
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

        return TypedResults.Created($"/api/artists/{artist.Id}", response);
    }

    [HttpPut]
    private static async Task<Results<Ok<ArtistResponseDTO>, NotFound<string>>> UpdateAsync(
        int id, ArtistUpdateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var artist = await db.Artists.Include(m => m.Musics)
            .ThenInclude(ma => ma.Music).FirstOrDefaultAsync(art => art.Id == id, cancellationToken);
        if (artist is null)
        {
            return TypedResults.NotFound($"Artist {id} not found.");
        }

        artist.Name = input.Name;
        await db.SaveChangesAsync(cancellationToken);

        ArtistResponseDTO response = ArtistResponse(artist);

        return TypedResults.Ok(response);
    }


    [HttpDelete]
    private static async Task<Results<NoContent, NotFound<string>>> DeleteAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var artist = await db.Artists.FindAsync([id], cancellationToken);
        if (artist is null)
            return TypedResults.NotFound($"Artist {id} not found.");

        db.Artists.Remove(artist);
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
