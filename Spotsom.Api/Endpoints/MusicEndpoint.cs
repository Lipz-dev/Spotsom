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
        .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

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
            CreateMusicDTO input,
            SpotsomContext db,
            CancellationToken cancellationToken)
    {
        // Verify if album exist
        var album = await db.Albums
            .FindAsync([input.AlbumId], cancellationToken);

        if (album is null)
        {
            return TypedResults.BadRequest(
                $"Álbum {input.AlbumId} não encontrado."
            );
        }

        // search all Artists
        var artists = await db.Artists
            .Where(a => input.ArtistIds.Contains(a.Id))
            .ToListAsync(cancellationToken);

        // Verify if Ids Artists sent exist
        if (artists.Count != input.ArtistIds.Count)
        {
            return TypedResults.BadRequest(
                "Um ou mais artistas informados não existem."
            );
        }

        // search all Genres
        var genres = await db.Genres
            .Where(g => input.GenreIds.Contains(g.Id))
            .ToListAsync(cancellationToken);

        // Verify if Ids Genres sent exist
        if (genres.Count != input.GenreIds.Count)
        {
            return TypedResults.BadRequest(
                "Um ou mais gêneros informados não existem."
            );
        }

        var music = new Music
        {
            Name = input.Name,
            Duration = input.Duration,
            ReleaseDate = input.ReleaseDate,
            AlbumId = input.AlbumId
        };

        // Create relationship Music <-> Artist
        foreach (var artist in artists)
        {
            music.Artists.Add(new MusicsArtists
            {
                ArtistsId = artist.Id,
                Artist = artist
            });
        }

        //Create relationship Music <-> Genre
        foreach (var genre in genres)
        {
            music.Genre.Add(new MusicGenres
            {
                GenreIds = genre.Id,
                Genre = genre
            });
        }

        db.Musics.Add(music);
        await db.SaveChangesAsync(cancellationToken);

        MusicResponseDTO response = MusicResponse(music);

        return TypedResults.Created(
            $"/api/musics/{music.Id}",
            response
        );
    }

    [HttpPut("{id}")]
    private static async Task<Results<Ok<MusicResponseDTO>, NotFound>> UpdateAsync(
        int id, UpdateMusicDTO input, SpotsomContext db, CancellationToken cancellationToken)
    {
        var music = await db.Musics
        .Include(a => a.Artists)
            .ThenInclude(am => am.Artist)
        .Include(al => al.Album)
        .Include(g => g.Genre)
            .ThenInclude(gm => gm.Genre).FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        var albumMus = await db.Albums.FindAsync([music.AlbumId], cancellationToken);

        if (music.Artists == null || music.Artists.Count() < 1)
        {
            return TypedResults.NotFound();
        }

        music.Name = input.Name;

        await db.SaveChangesAsync(cancellationToken);

        MusicResponseDTO response = MusicResponse(music);

        return TypedResults.Ok(response);
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

    private static MusicResponseDTO MusicResponse(Music music)
    {
        return new MusicResponseDTO
        (
        music.Name,
        music.Duration,
        music.ReleaseDate,
        music.Artists.Select(g => g.Artist.Name).ToList(),
        music.Genre.Select(g => g.Genre.Name).ToList(),
        music.Album.Name
        );
    }
}