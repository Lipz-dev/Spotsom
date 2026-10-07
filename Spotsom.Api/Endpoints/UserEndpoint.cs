using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Data;
using SpotSom.Api.Models;
using SpotSom.Api.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Spotsom.Api.Endpoints;

public static class UserEndpoint
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/create", CreateAsync);
        group.MapPut("/{id:int}/edit", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }

    [HttpGet]
    private static async Task<Ok<List<UserResponseDTO>>> GetAllAsync(
        SpotsomContext db, CancellationToken cancellationToken)
    {
        var users = await db.Users.ToListAsync(cancellationToken);

        List<UserResponseDTO> response = users.Select(u => UserResponse(u)).ToList();
        return TypedResults.Ok(response);
    }


    [HttpGet]
    private static async Task<Results<Ok<UserResponseDTO>, NotFound>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);

        UserResponseDTO response = UserResponse(user);
        return user is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    [HttpPost]
    private static async Task<Created<UserResponseDTO>> CreateAsync(
        UserCreateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        User user = new User
        {
            Name = input.Name,
            ReleaseDate = input.ReleaseDate
        };

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        UserResponseDTO response = UserResponse(user);

        var id = user.GetType().GetProperty("Id")?.GetValue(user);
        return TypedResults.Created($"/api/users/{id}", response);
    }

    [HttpPut]
    private static async Task<Results<Ok<UserResponseDTO>, NotFound>> UpdateAsync(
        int id, UserUpdateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null)
        {
            return TypedResults.NotFound();
        }


        user.Name = input.Name;
        await db.SaveChangesAsync(cancellationToken);

        UserResponseDTO response = UserResponse(user);

        return TypedResults.Ok(response);
    }

    [HttpDelete]
    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);
        if (user is null)
        {
            return TypedResults.NotFound();
        }


        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }

    private static UserResponseDTO UserResponse(User user)
    {
        return new UserResponseDTO(
            user.Name,
            user.ReleaseDate
        );
    }
}
