using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Data;
using SpotSom.Api.Models;
using SpotSom.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Spotsom.Api.Endpoints;

public static class GenreEndpoint
{
    public static IEndpointRouteBuilder MapGenreEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/genres").WithTags("Genres");

        group.MapGet("/page={page:int}/size={pageSize:int}", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/create", CreateAsync);
        group.MapPut("/{id:int}/edit", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }

    [HttpGet]
    private static async Task<Ok<PaginetedDto<GenreResponseDTO>>> GetAllAsync(
        int page, int pageSize, SpotsomContext db, CancellationToken cancellationToken)
    {
        var genres = await db.Genres.OrderBy(g => g.Name)
        .Take(pageSize * page)
        .Include(m => m.Musics)
            .ThenInclude(mg => mg.Music).ToListAsync(cancellationToken);

        int totalItems = await db.Genres.CountAsync(cancellationToken);


        var response = new PaginetedDto<GenreResponseDTO>(genres.Select(g => GenreResponse(g)
        ).ToList(),
        page,
        pageSize,
        totalItems,
        0);

        return TypedResults.Ok(response);
    }

    [HttpGet]
    private static async Task<Results<Ok<GenreResponseDTO>, NotFound>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var genre = await db.Genres.Include(m => m.Musics)
            .ThenInclude(mg => mg.Music).FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

        GenreResponseDTO response = GenreResponse(genre);

        return genre is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }


    [HttpPost]
    private static async Task<Created<GenreResponseDTO>> CreateAsync(
        GenreCreateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        Genre genre = new()
        {
            Name = input.Name
        };

        db.Genres.Add(genre);
        await db.SaveChangesAsync(cancellationToken);

        GenreResponseDTO response = GenreResponse(genre);

        var id = genre.GetType().GetProperty("Id")?.GetValue(genre);
        return TypedResults.Created($"/api/genres/{id}", response);
    }


    [HttpPut]
    private static async Task<Results<Ok<GenreResponseDTO>, NotFound>> UpdateAsync(
        int id, GenreUpdateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var genre = await db.Genres.Include(m => m.Musics).FirstOrDefaultAsync(g => g.Id == id, cancellationToken);
        if (genre is null)
            return TypedResults.NotFound();

        genre.Name = input.Name;
        await db.SaveChangesAsync(cancellationToken);

        GenreResponseDTO response = GenreResponse(genre);

        return TypedResults.Ok(response);
    }


    [HttpDelete]
    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var genre = await db.Genres.FindAsync([id], cancellationToken);
        if (genre is null)
            return TypedResults.NotFound();

        db.Genres.Remove(genre);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }


    private static GenreResponseDTO GenreResponse(Genre genre)
    {
        GenreResponseDTO responseDTO = new(genre.Name, genre.Musics.Select(m => m.Music.Name).ToList());
        return responseDTO;
    }
}
