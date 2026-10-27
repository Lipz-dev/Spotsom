using SpotSom.Api.Dtos;
using SpotSom.Api.Data;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Spotsom.Api.Services;
using SpotSom.Api.Services;
using System.Runtime.CompilerServices;


namespace SpotSom.Api.Endpoints;

public static class MusicEndpoint
{
    public static IEndpointRouteBuilder MapMusicEndpoint(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/musics").WithTags("Musics");

        group.MapGet("/page={page:int}/size={pageSize:int}", GetAllAsync);
        group.MapGet("/{id:int}", GetByIdAsync);
        group.MapGet("/player/{id:int}", GetPlayerAsync);
        group.MapPost("/create", CreatedAsync).DisableAntiforgery();
        group.MapPut("/{id:int}/edit/", UpdateAsync);
        group.MapDelete("/{id:int}", DeleteAsync);

        return endpoints;
    }

    [HttpGet]
    private static async Task<Results<Ok<PaginetedDto<MusicResponseDTO>>, NotFound<string>>> GetAllAsync(
        int page, int pageSize,
        SpotsomContext db, CancellationToken cancellationToken)
    {
        if (await db.Musics.CountAsync() < 1)
        {
            return TypedResults.NotFound("Nothing music added");
        }

        var musics = await db.Musics
        .OrderBy(m => m.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Include(m => m.Artists)
            .ThenInclude(ma => ma.Artist)
        .Include(g => g.Genre)
            .ThenInclude(mg => mg.Genre)
        .Include(a => a.Album).ToListAsync(cancellationToken);

        int totalItems = await db.Musics.CountAsync(cancellationToken);
        int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

        var response = new PaginetedDto<MusicResponseDTO>(
            musics.Select(m => MusicResponse(m)).ToList(),
            page,
            pageSize,
            totalItems,
            totalPages
        );

        return TypedResults.Ok(response);
    }


    [HttpGet]
    private static async Task<Results<Ok<MusicResponseDTO>, NotFound<string>, BadRequest<string>>> GetByIdAsync(
        int id, SpotsomContext db, CancellationToken cancellationToken)
    {


        var music = await db.Musics
        .Include(m => m.Artists).ThenInclude(ma => ma.Artist)
        .Include(g => g.Genre).ThenInclude(mg => mg.Genre)
        .Include(a => a.Album)
        .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);


        if (music is null)
        {
            return TypedResults.NotFound($"Music {id} not found.");
        }

        if (music.Artists == null || music.Genre == null || music.Album == null)
        {
            return TypedResults.BadRequest("Erro ao buscar a música. Verifique se os IDs de artistas e gêneros são válidos.");
        }


        MusicResponseDTO response = MusicResponse(music);

        return TypedResults.Ok(response);
    }

    [HttpGet] //Music Player
    private static async Task<IResult> GetPlayerAsync(
        int id,
        SpotsomContext db,
        IWebHostEnvironment environment,
        CancellationToken cancellationToken)
    {

        var audioPath = await db.Musics
            .Where(m => m.Id == id)
            .Select(m => m.AudioPath)
            .FirstOrDefaultAsync(cancellationToken);

        if (audioPath is null)
        {
            return TypedResults.NotFound($"Music {id} not found.");
        }

        var storedPath = Uri.TryCreate(audioPath, UriKind.Absolute, out var uri) && uri.IsFile
            ? uri.LocalPath
            : audioPath;
        var fileName = Path.GetFileName(storedPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return TypedResults.NotFound($"Audio for music {id} not found.");
        }

        var filePath = Path.Combine(
            environment.ContentRootPath,
            "wwwroot",
            "audio",
            fileName
        );
        return File.Exists(filePath)
            ? TypedResults.PhysicalFile(filePath, "audio/mpeg", enableRangeProcessing: true)
            : TypedResults.NotFound($"Audio for music {id} not found.");
    }

    [HttpPost] //Create Music
    private static async Task<Results<Created<MusicResponseDTO>, BadRequest<string>>>
     CreatedAsync(
            [FromForm] CreateMusicDTO input,
            MusicService musicService,
            AudioStorageService audioStorage,
            SpotsomContext db,
            CancellationToken cancellationToken)
    {
        var musicExists = await musicService.MusicExists(cancellationToken, input.Audio.FileName);
        if (musicExists.Contains("already exists"))
        {
            return TypedResults.BadRequest(musicExists);
        }

        var album = await db.Albums.FindAsync([1], cancellationToken);
        var artist = await db.Artists.FindAsync([1], cancellationToken);
        var genre = await db.Genres.FindAsync([1], cancellationToken);
        if (album is null || artist is null || genre is null)
        {
            return TypedResults.BadRequest("Erro ao criar a música. Verifique se os IDs de artistas e gêneros são válidos.");
        }

        var audioPath = await audioStorage.SaveAsync(
            input.Audio,
            cancellationToken
        );

        var music = new Music
        {
            Name = input.Audio.FileName,
            AudioPath = audioPath,
            Duration = input.Audio.Length > 0 ? TimeSpan.FromSeconds(input.Audio.Length / 1000.0) : TimeSpan.Zero,
            ReleaseDate = DateOnly.FromDateTime(DateTime.Now),
            AlbumId = album.Id,
            Album = album
        };

        music.Artists.Add(new MusicsArtists
        {
            ArtistsId = artist.Id,
            Artist = artist
        });

        music.Genre.Add(new MusicGenres
        {
            GenreIds = genre.Id,
            Genre = genre
        });

        db.Musics.Add(music);
        await db.SaveChangesAsync(cancellationToken);

        MusicResponseDTO response = MusicResponse(music);

        return TypedResults.Created(
            $"/api/musics/{music.Id}",
            response
        );
    }

    [HttpPut("{id}")] //Update Music
    private static async Task<Results<Ok<MusicResponseDTO>, NotFound<string>, BadRequest<string>>> UpdateAsync(
        int id, UpdateMusicDTO input, SpotsomContext db,
        MusicService musicService, CancellationToken cancellationToken)
    {
        var music = await db.Musics
        .Include(a => a.Artists)
            .ThenInclude(am => am.Artist)
        .Include(al => al.Album)
        .Include(g => g.Genre)
            .ThenInclude(gm => gm.Genre).AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (music is null)
        {
            return TypedResults.NotFound($"Music {id} not found.");
        }

        // if (music.Artists == null || music.Artists.Count() < 1)
        // {
        //     return TypedResults.NotFound($"Artists for music {id} not found.");
        // }


        var musicUpdateResponse = await musicService.UpdateMusicAsync(music, input, cancellationToken);

        if (musicUpdateResponse is null)
        {
            return TypedResults.BadRequest("Album, artist, or genre ID does not exist.");
        }

        return TypedResults.Ok(musicUpdateResponse);
    }

    [HttpDelete("{id}")]
    private static async Task<Results<NoContent, NotFound<string>>> DeleteAsync(
    int id, SpotsomContext db, CancellationToken cancellationToken)
    {
        var music = await db.Musics.FindAsync([id], cancellationToken);
        if (music is null)
        {
            return TypedResults.NotFound($"Music {id} not found.");
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
        music.Artists.Select(a => a.Artist.Name).ToList(),
        music.Genre.Select(g => g.Genre.Name).ToList(),
        music.Album.Name,
        $"/api/musics/player/{music.Id}"
        );
    }
}