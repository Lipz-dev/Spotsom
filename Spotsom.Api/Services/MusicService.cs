using SpotSom.Api.Dtos;
using SpotSom.Api.Data;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Models;
using System.IO;

namespace SpotSom.Api.Services;

/// <summary>
/// Provides music-specific validation, metadata, and relationship-update operations.
/// </summary>
public class MusicService(SpotsomContext context, IWebHostEnvironment environment)
{
    private SpotsomContext _context = context;
    private IWebHostEnvironment _environment = environment;

    /// <summary>
    /// Checks that the album, artists, and genres referenced by a new music entity exist.
    /// </summary>
    /// <param name="input">The music entity to validate.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True if all referenced entities exist; otherwise, false.</returns>
    public async Task<bool> VerifyCreateMusicAsync(Music input, CancellationToken cancellationToken)
    {
        var album = await _context.Albums
            .FirstOrDefaultAsync(a => a.Id == input.AlbumId, cancellationToken);

        if (album is null)
        {
            return false;
        }
        foreach (var artistId in input.Artists.Select(am => am.ArtistsId))
        {
            var artist = await _context.Artists
                .FirstOrDefaultAsync(a => a.Id == artistId, cancellationToken);

            if (artist is null || artistId <= 0)
            {
                return false;
            }
        }

        foreach (var genreId in input.Genres.Select(gm => gm.GenresId))
        {
            var genre = await _context.Genres
                .FirstOrDefaultAsync(g => g.Id == genreId, cancellationToken);

            if (genre is null || genreId <= 0)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Determines whether the supplied name is not already used by a music record.
    /// The comparison is case-insensitive.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param> 
    /// <param name="name">The name to check for existence.</param>
    /// <returns>True if a music with the same name exists; otherwise, false.</returns>
    public async Task<bool> MusicAlreadyExistsAsync(CancellationToken cancellationToken, string name)
    {
        var musicTeste = await _context.Musics.Select(m => m.Name.ToLower()).ToListAsync(cancellationToken);
        var musicFilter = musicTeste.Where(m => m == name.ToLower());

        return musicFilter.Any();
    }

    /// <summary>
    /// Reads the duration of an MP3 stored under the application's public audio folder.
    /// Corrupt files are removed because they cannot be played reliably.
    /// </summary>
    /// <param name="audioPath">The relative path to the audio file.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The duration of the audio file, or null if the file is missing or corrupt.</returns>
    public async Task<TimeSpan?> GetMusicDurationAsync(string audioPath, IWebHostEnvironment environment, CancellationToken cancellationToken)
    {

        var fileName = Path.GetFileName(audioPath);
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var filePath = Path.Combine(
            environment.ContentRootPath,
            "wwwroot",
            "audio",
            fileName
        );

        if (!File.Exists(filePath))
        {
            return null;
        }


        try
        {
            using var audioFile = TagLib.File.Create(filePath);
            return audioFile.Properties.Duration;
        }
        catch (TagLib.CorruptFileException)
        {
            File.Delete(filePath);
            return null;
        }

    }

    /// <summary>
    /// Updates editable music metadata and synchronizes its artist and genre relationships.
    /// </summary>
    /// <param name="music">The music entity to update.</param>
    /// <param name="input">The DTO containing the new metadata and relationship IDs.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>A DTO representing the updated music, or null if the music was not found.</returns>
    public async Task<MusicResponseDTO?> UpdateMusicAsync(Music music, UpdateMusicDTO input, CancellationToken cancellationToken)
    {
        var artistIds = input.ArtistIds.Distinct().ToHashSet();
        var genreIds = input.GenreIds.Distinct().ToHashSet();
        var album = await _context.Albums
            .FirstOrDefaultAsync(a => a.Id == input.AlbumId, cancellationToken);
        var artists = await _context.Artists
            .Where(a => artistIds.Contains(a.Id))
            .ToListAsync(cancellationToken);
        var genres = await _context.Genres
            .Where(g => genreIds.Contains(g.Id))
            .ToListAsync(cancellationToken);


        music.Name = input.Name;
        music.AlbumId = input.AlbumId;
        music.Album = album;

        var currentArtistIds = music.Artists
            .Select(a => a.ArtistsId)
            .ToHashSet();

        foreach (var relation in music.Artists
            .Where(a => !artistIds.Contains(a.ArtistsId))
            .ToList())
        {
            music.Artists.Remove(relation);
        }

        foreach (var artist in artists.Where(a => !currentArtistIds.Contains(a.Id)))
        {
            music.Artists.Add(new MusicsArtists
            {
                MusicsId = music.Id,
                ArtistsId = artist.Id,
                Artist = artist
            });
        }

        var currentGenreIds = music.Genres
            .Select(g => g.GenresId)
            .ToHashSet();

        foreach (var relation in music.Genres
            .Where(g => !genreIds.Contains(g.GenresId))
            .ToList())
        {
            music.Genres.Remove(relation);
        }

        foreach (var genre in genres.Where(g => !currentGenreIds.Contains(g.Id)))
        {
            music.Genres.Add(new MusicGenres
            {
                MusicsId = music.Id,
                GenresId = genre.Id,
                Genre = genre
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        var musicResponse = await _context.Musics.Include(a => a.Artists)
            .ThenInclude(am => am.Artist)
        .Include(g => g.Genres)
            .ThenInclude(gm => gm.Genre)
        .Include(al => al.Album)
        .FirstAsync(m => m.Id == music.Id, cancellationToken);

        return new MusicResponseDTO(
            musicResponse.Name,
            musicResponse.Duration,
            musicResponse.ReleaseDate,
            musicResponse.Artists.Select(a => a.Artist.Name).ToList(),
            musicResponse.Genres.Select(g => g.Genre.Name).ToList(),
            musicResponse.Album.Name,
            musicResponse.AudioPath

        );
    }

    /// <summary>
    /// Gets the file path of the audio file for a given music ID.
    /// </summary>
    /// <param name="musicId">The ID of the music.</param>
    /// <param name="environment">The web host environment.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The file path of the audio file, or null if the music or file does not exist.</returns>
    public async Task<string?> GetAudioFilePathAsync(
        int musicId,
        CancellationToken cancellationToken)
    {
        var audioPath = await _context.Musics
            .Where(music => music.Id == musicId)
            .Select(music => music.AudioPath)
            .FirstOrDefaultAsync(cancellationToken);

        if (string.IsNullOrWhiteSpace(audioPath))
        {
            return null;
        }

        var storedPath = Uri.TryCreate(audioPath, UriKind.Absolute, out var uri) && uri.IsFile
            ? uri.LocalPath
            : audioPath;

        var fileName = Path.GetFileName(storedPath);

        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        var filePath = Path.Combine(
            _environment.ContentRootPath,
            "wwwroot",
            "audio",
            fileName);

        return File.Exists(filePath) ? filePath : null;
    }
}