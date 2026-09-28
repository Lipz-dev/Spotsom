using SpotSom.Api.Dtos;
using SpotSom.Api.Data;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace SpotSom.Api.Endpoints;

public static class MusicEndpoint
{
    public static IEndpointRouteBuilder MapMusicEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/musics").WithTags("Musics");

        group.MapGet("/", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapPost("/create", CreatedAsync);
        group.MapPut("/{id:int}/edit/", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }

    [HttpGet]
    private static async Task<Ok<List<MusicResponseDTO>>> GetAllAsync(
        SpotsomContext db, CancellationToken cancellationToken)
    {
        var musics = await db.Musics
        .Include(m => m.Artists)
            .ThenInclude(ma => ma.Artist)
        .Include(g => g.Genre)
            .ThenInclude(mg => mg.Genre)
        .Include(a => a.Album).ToListAsync(cancellationToken);


        List<MusicResponseDTO> musicResponseDTOs = musics.Select(m => new MusicResponseDTO(
            m.Name,
            m.Duration,
            m.ReleaseDate,
            m.Artists.Select(a => a.Artist.Name).ToList(),
            m.Genre.Select(g => g.Genre.Name).ToList(),
            m.Album.Name)).ToList();
        return TypedResults.Ok(musicResponseDTOs);
    }


    [HttpGet]
    private static async Task<Results<Ok<MusicResponseDTO>, NotFound, BadRequest<string>>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {

        var music = await db.Musics
        .Include(m => m.Artists).ThenInclude(ma => ma.Artist)
        .Include(g => g.Genre).ThenInclude(mg => mg.Genre)
        .Include(a => a.Album)
        .FirstOrDefaultAsync(m => m.Id == id);

        if (music.Artists == null || music.Genre == null || music.Album == null)
        {
            return TypedResults.BadRequest("Erro ao buscar a música. Verifique se os IDs de artistas e gêneros são válidos.");
        }

        MusicResponseDTO response = new MusicResponseDTO(
            music.Name,
            music.Duration,
            music.ReleaseDate,
            music.Artists.Select(a => a.Artist.Name).ToList(),
            music.Genre.Select(g => g.Genre.Name).ToList(),
            music.Album.Name);

        return music is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    [HttpPost]
    private static async Task<Results<Created<MusicResponseDTO>, BadRequest<string>>> CreatedAsync(
        CreateMusicDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var album = await db.Albums.FindAsync([input.AlbumId], cancellationToken);
        if (album is null)
        {
            return TypedResults.BadRequest($"Album {input.AlbumId} não encontrado.");
        }

        var music = new Music
        {
            Name = input.Name,
            Duration = input.Duration,
            ReleaseDate = input.ReleaseDate,
            AlbumId = input.AlbumId
        };



        foreach (var artistId in input.ArtistIds)
        {
            db.MusicsArtists.Add(new MusicsArtists
            {
                Music = music,
                ArtistsId = artistId,
                Artist = await db.Artists.FindAsync([artistId], cancellationToken)
            });
        }

        foreach (var genreId in input.GenreIds)
        {
            db.MusicGenres.Add(new MusicGenres
            {
                Music = music,
                GenreIds = genreId,
                Genre = await db.Genres.FindAsync([genreId], cancellationToken)
            });
        }

        db.Musics.Add(music);
        await db.SaveChangesAsync(cancellationToken);

        if (music.Artists == null || music.Genre == null || music.Album == null)
        {
            return TypedResults.BadRequest("Erro ao criar a música. Verifique se os IDs de artistas e gêneros são válidos.");
        }

        MusicResponseDTO response = new MusicResponseDTO(
            music.Name,
            music.Duration,
            music.ReleaseDate,
            music.Artists.Select(a => a.Artist.Name).ToList(),
            music.Genre.Select(g => g.Genre.Name).ToList(),
            music.Album.Name);

        var id = music.Id;
        return TypedResults.Created($"/api/musics/{id}", response);
    }

    [HttpPut("{id}")]
    private static async Task<Results<Ok<Music>, NotFound>> UpdateAsync(
        int id, UpdateMusicDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var music = await db.Musics.FindAsync([id], cancellationToken);
        if (music is null)
        {
            return TypedResults.NotFound();
        }

        db.Entry(music).CurrentValues.SetValues(input);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.Ok(music);
    }

    [HttpDelete("{id}")]
    private static async Task<Results<NoContent, NotFound>> DeleteAsync(
    int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var music = await db.Musics.FindAsync([id], cancellationToken);
        if (music is null)
        {
            return TypedResults.NotFound();
        }

        db.Musics.Remove(music);
        await db.SaveChangesAsync(cancellationToken);
        return TypedResults.NoContent();
    }
}