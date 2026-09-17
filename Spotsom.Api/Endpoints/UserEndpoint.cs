using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Data;
using SpotSom.Api.Models;
using SpotSom.Api.Dtos;

namespace Spotsom.Api.Endpoints;

public static class UserEndpoint
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/create", CreateAsync);
        group.MapPut("/{id:int}", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }

    private static async Task<Ok<List<User>>> GetAllAsync(
        SpotsomContext db, CancellationToken cancellationToken) =>
        TypedResults.Ok(await db.Users.ToListAsync(cancellationToken));

    private static async Task<Results<Ok<User>, NotFound>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);
        return user is null ? TypedResults.NotFound() : TypedResults.Ok(user);
    }

    private static async Task<Created<User>> CreateAsync(
        User user, SpotsomContext db, CancellationToken cancellationToken)
    {
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        var id = user.GetType().GetProperty("Id")?.GetValue(user);
        return TypedResults.Created($"/api/users/{id}", user);
    }

    private static async Task<Results<Ok<User>, NotFound>> UpdateAsync(
        int id, UserUpdateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null)
            return TypedResults.NotFound();

        db.Entry(user).CurrentValues.SetValues(input);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(user);
    }

    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null)
            return TypedResults.NotFound();

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }
}
