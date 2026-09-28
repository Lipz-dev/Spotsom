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

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/create", CreateAsync);
        group.MapPut("/{id:int}/edit", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }

    [HttpGet]
    private static async Task<Ok<List<Genre>>> GetAllAsync(
        SpotsomContext db, CancellationToken cancellationToken) =>
        TypedResults.Ok(await db.Genres.ToListAsync(cancellationToken));


    [HttpGet]
    private static async Task<Results<Ok<Genre>, NotFound>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var genre = await db.Genres.FindAsync([id], cancellationToken);
        return genre is null ? TypedResults.NotFound() : TypedResults.Ok(genre);
    }


    [HttpPost]
    private static async Task<Created<Genre>> CreateAsync(
        GenreCreateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        Genre genre = new Genre
        {
            Name = input.Name
        };

        db.Genres.Add(genre);
        await db.SaveChangesAsync(cancellationToken);

        var id = genre.GetType().GetProperty("Id")?.GetValue(genre);
        return TypedResults.Created($"/api/genres/{id}", genre);
    }


    [HttpPut]
    private static async Task<Results<Ok<Genre>, NotFound>> UpdateAsync(
        int id, GenreUpdateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var genre = await db.Genres.FindAsync([id], cancellationToken);
        if (genre is null)
            return TypedResults.NotFound();

        db.Entry(genre).CurrentValues.SetValues(input);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(genre);
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
}
