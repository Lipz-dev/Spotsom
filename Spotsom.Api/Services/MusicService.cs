using SpotSom.Api.Dtos;
using SpotSom.Api.Data;
using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Models;

namespace SpotSom.Api.Services;

public class MusicService(SpotsomContext context)
{
    private SpotsomContext _context = context;

    public async Task<bool> VerifyCreateMusic(CancellationToken cancellationToken)
    {
        var album = await _context.Albums
            .FirstOrDefaultAsync(a => a.Id == 1, cancellationToken);

        if (album is null)
        {
            return false;
        }

        var artists = await _context.Artists
            .Where(a => a.Id == 1)
            .ToListAsync(cancellationToken);

        if (artists is null || artists.Count() < 1)
        {
            return false;
        }

        var genres = await _context.Genres
            .Where(g => g.Id == 1)
            .ToListAsync(cancellationToken);

        if (genres is null || genres.Count() < 1)
        {
            return false;
        }

        return true;
    }

    public async Task<string> MusicExists(CancellationToken cancellationToken, string name)
    {
        var musicTeste = await _context.Musics.Select(m => m.Name.ToLower()).ToListAsync(cancellationToken);
        var musicFilter = musicTeste.Where(m => m == name.ToLower());

        return !musicFilter.Any() ? $"Music {name} does not exist." : $"Music {name} already exists.";
    }

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

        // if (album is null || artists.Count != artistIds.Count || genres.Count != genreIds.Count)
        // {
        //     return null;
        // }

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
                MusicId = music.Id,
                ArtistsId = artist.Id,
                Artist = artist
            });
        }

        var currentGenreIds = music.Genre
            .Select(g => g.GenreIds)
            .ToHashSet();

        foreach (var relation in music.Genre
            .Where(g => !genreIds.Contains(g.GenreIds))
            .ToList())
        {
            music.Genre.Remove(relation);
        }

        foreach (var genre in genres.Where(g => !currentGenreIds.Contains(g.Id)))
        {
            music.Genre.Add(new MusicGenres
            {
                MusicId = music.Id,
                GenreIds = genre.Id,
                Genre = genre
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        var musicResponse = await _context.Musics.Include(a => a.Artists)
            .ThenInclude(am => am.Artist)
        .Include(g => g.Genre)
            .ThenInclude(gm => gm.Genre)
        .Include(al => al.Album)
        .FirstAsync(m => m.Id == music.Id, cancellationToken);

        return new MusicResponseDTO(
            musicResponse.Name,
            musicResponse.Duration,
            musicResponse.ReleaseDate,
            musicResponse.Artists.Select(a => a.Artist.Name).ToList(),
            musicResponse.Genre.Select(g => g.Genre.Name).ToList(),
            musicResponse.Album.Name,
            musicResponse.AudioPath

        );
    }

    internal void UpdateMusicAsync(Music music, UpdateMusicDTO input)
    {
        throw new NotImplementedException();
    }
}