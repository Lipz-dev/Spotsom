using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Data;
using SpotSom.Api.Models;
using SpotSom.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace Spotsom.Api.Endpoints;

public static class UserEndpoint
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/page={page:int}/size={pageSize:int}", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/create", CreateAsync);
        group.MapPut("/{id:int}/edit", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }

    [HttpGet]
    private static async Task<Results<Ok<PaginetedDto<UserResponseDTO>>, NotFound<string>>> GetAllAsync(
        int page, int pageSize, SpotsomContext db, CancellationToken cancellationToken)
    {
        if (await db.Users.CountAsync(cancellationToken) <= 0)
        {
            return TypedResults.NotFound("No Users Created");
        }
        var users = await db.Users.OrderBy(u => u.Id)
            .Take(pageSize * page)
            .ToListAsync(cancellationToken);

        int totalItems = await db.Users.CountAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var response = new PaginetedDto<UserResponseDTO>(
            users.Select(u => UserResponse(u)).ToList(),
            page,
            pageSize,
            totalItems,
            totalPages);

        return TypedResults.Ok(response);
    }


    [HttpGet]
    private static async Task<Results<Ok<UserResponseDTO>, NotFound<string>>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken);

        if (user is null)
        {
            return TypedResults.NotFound($"User {id} not found.");
        }

        UserResponseDTO response = UserResponse(user);
        return TypedResults.Ok(response);
    }

    [HttpPost]
    private static async Task<Results<Created<UserResponseDTO>, BadRequest<string>>> CreateAsync(
        UserCreateDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        User user = new User
        {
            Name = input.Name,
            ReleaseDate = DateOnly.FromDateTime(DateTime.UtcNow)
        };

        if (user is not null)
        {
            var userTeste = await db.Users.Select(m => m.Name.ToLower()).ToListAsync(cancellationToken);
            var userFilter = userTeste.Where(m => m == input.Name.ToLower());

            if (userFilter.Count() > 0)
            {
                return TypedResults.BadRequest($"User {input.Name} already exists.");
            }
        }


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
