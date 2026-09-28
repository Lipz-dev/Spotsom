using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Models;

namespace SpotSom.Api.Data;

public class SpotsomContext(DbContextOptions<SpotsomContext> options)
: DbContext(options)
{
    public DbSet<Music> Musics => Set<Music>();
    public DbSet<MusicsArtists> MusicsArtists => Set<MusicsArtists>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<MusicGenres> MusicGenres => Set<MusicGenres>();
    public DbSet<Artist> Artists => Set<Artist>();

    public DbSet<Playlist> Playlists => Set<Playlist>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MusicGenres>().HasKey(mg => new { mg.GenreIds, mg.MusicId });
        modelBuilder.Entity<MusicsArtists>().HasKey(ma => new { ma.ArtistsId, ma.MusicId });
    }
}