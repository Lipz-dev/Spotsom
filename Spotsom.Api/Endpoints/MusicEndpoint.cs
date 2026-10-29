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

/// <summary>
/// Registers music routes, including upload, streaming, metadata updates, and deletion.
/// </summary>
public static class MusicEndpoint
{
    /// <summary>
    /// Maps all music endpoints under the <c>/api/musics</c> route group.
    /// </summary>
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
    private static async Task<Results<Ok<PaginetedDto<MusicResponseDTO>>, BadRequest<string>, NotFound<string>>> GetAllAsync(
        int page, int pageSize,
        SpotsomContext db, CancellationToken cancellationToken)
    {
        if (await db.Musics.CountAsync() < 1)
        {
            return TypedResults.NotFound("Nothing music added");
        }
        if (page <= 0 || pageSize <= 0)
        {
            return TypedResults.BadRequest("Page and pageSize must be greater than zero.");
        }

        var musics = await db.Musics
        .OrderBy(m => m.Name)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Include(m => m.Artists)
            .ThenInclude(ma => ma.Artist)
        .Include(g => g.Genres)
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
        .Include(g => g.Genres).ThenInclude(mg => mg.Genre)
        .Include(a => a.Album)
        .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);


        if (music is null)
        {
            return TypedResults.NotFound($"Music {id} not found.");
        }

        if (music.Artists == null || music.Genres == null || music.Album == null)
        {
            return TypedResults.BadRequest("Error searching for the music. Verify that the artist and genre IDs are valid.");
        }


        MusicResponseDTO response = MusicResponse(music);

        return TypedResults.Ok(response);
    }

    // Streams the file with range-request support so clients can seek and resume playback.
    [HttpGet]
    private static async Task<IResult> GetPlayerAsync(
        int id,
        SpotsomContext db,
        MusicService musicService,
        CancellationToken cancellationToken)
    {

        var filePath = await musicService.GetAudioFilePathAsync(id, cancellationToken);

        if (filePath is null)
        {
            return TypedResults.NotFound($"Audio for music {id} not found.");
        }

        return TypedResults.PhysicalFile(
            filePath,
            "audio/mpeg",
            enableRangeProcessing: true);
    }

    // Multipart upload: saves the audio file first, then persists the track metadata.
    [HttpPost]
    private static async Task<Results<Created<MusicResponseDTO>, BadRequest<string>>>
     CreatedAsync(
            [FromForm] CreateMusicDTO input,
            MusicService musicService,
            AudioStorageService audioStorage,
            SpotsomContext db,
            IWebHostEnvironment environment,
            CancellationToken cancellationToken)
    {
        var musicExists = await musicService.MusicAlreadyExistsAsync(cancellationToken, input.Audio.FileName);
        if (musicExists)
        {
            return TypedResults.BadRequest("Music already exists.");
        }

        var album = await db.Albums.FindAsync([1], cancellationToken);
        var artist = await db.Artists.FindAsync([1], cancellationToken);
        var genre = await db.Genres.FindAsync([1], cancellationToken);
        if (album is null || artist is null || genre is null)
        {
            return TypedResults.BadRequest("Error creating the music. Verify that the artist and genre IDs are valid.");
        }

        var audioPath = await audioStorage.SaveAsync(
            input.Audio,
            cancellationToken
        );

        var duration = await musicService.GetMusicDurationAsync(audioPath, environment, cancellationToken);

        if (duration is null)
        {
            return TypedResults.BadRequest("Error getting the music duration.");
        }

        var music = new Music
        {
            Name = input.Audio.FileName,
            AudioPath = audioPath,
            Duration = (TimeSpan)duration,
            ReleaseDate = DateOnly.FromDateTime(DateTime.Now),
            AlbumId = album.Id,
            Album = album
        };

        music.Artists.Add(new MusicsArtists
        {
            ArtistsId = artist.Id,
            Artist = artist
        });

        music.Genres.Add(new MusicGenres
        {
            GenresId = genre.Id,
            Genre = genre
        });

        if (!await musicService.VerifyCreateMusicAsync(music, cancellationToken))
        {
            return TypedResults.BadRequest("Error creating the music. Verify that the artist and genre IDs are valid.");
        }

        music.Artists.Clear();
        music.Genres.Clear();

        db.Musics.Add(music);
        await db.SaveChangesAsync(cancellationToken);

        db.MusicsArtists.Add(new MusicsArtists
        {
            MusicsId = music.Id,
            ArtistsId = artist.Id,
            Artist = artist,
            Music = music
        });
        db.MusicGenres.Add(new MusicGenres
        {
            MusicsId = music.Id,
            GenresId = genre.Id,
            Genre = genre,
            Music = music
        });
        await db.SaveChangesAsync(cancellationToken);

        MusicResponseDTO response = MusicResponse(music);

        return TypedResults.Created(
            $"/api/musics/{music.Id}",
            response
        );
    }

    // Updates metadata and synchronizes the track's many-to-many relationships.
    [HttpPut("{id}")]
    private static async Task<Results<Ok<MusicResponseDTO>, NotFound<string>, BadRequest<string>>> UpdateAsync(
        int id, UpdateMusicDTO input, SpotsomContext db,
        MusicService musicService, CancellationToken cancellationToken)
    {
        var music = await db.Musics
        .Include(a => a.Artists)
            .ThenInclude(am => am.Artist)
        .Include(al => al.Album)
        .Include(g => g.Genres)
            .ThenInclude(gm => gm.Genre).AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (music is null)
        {
            return TypedResults.NotFound($"Music {id} not found.");
        }

        // The service adds and removes join records so stale artist links are not retained.
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
        var music = await db.Musics
            .Include(m => m.Artists)
            .ThenInclude(ma => ma.Artist)
            .Include(m => m.Genres)
            .ThenInclude(mg => mg.Genre)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        if (music is null)
        {
            return TypedResults.NotFound($"Music {id} not found.");
        }

        // Load playlists and relationship records before deleting the track.
        // SQLite rejects the delete while any foreign key references the music.
        var playlists = await db.Playlists
            .Include(p => p.Musics)
            .Where(p => p.Musics.Any(m => m.Id == id))
            .ToListAsync(cancellationToken);

        foreach (var playlist in playlists)
        {
            playlist.Musics.Remove(music);
        }

        music.Artists.Clear();
        music.Genres.Clear();

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
        music.Genres.Select(g => g.Genre.Name).ToList(),
        music.Album.Name,
        $"/api/musics/player/{music.Id}"
        );
    }
}