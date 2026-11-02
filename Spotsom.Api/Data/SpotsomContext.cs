using Microsoft.EntityFrameworkCore;
using SpotSom.Api.Models;

namespace SpotSom.Api.Data;

/// <summary>
/// EF Core database context for Spotsom music library entities and relationships.
/// </summary>
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
        modelBuilder.Entity<MusicGenres>()
            .HasKey(mg => new { mg.GenresId, mg.MusicsId });

        modelBuilder.Entity<MusicsArtists>()
            .HasKey(ma => new { ma.ArtistsId, ma.MusicsId });

        modelBuilder.Entity<Album>()
            .HasMany(m => m.Musics)
            .WithOne(a => a.Album)
            .HasForeignKey(m => m.AlbumId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Artist>()
            .HasMany(a => a.Albums)
            .WithOne(al => al.Artist)
            .HasForeignKey(al => al.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MusicsArtists>()
            .HasOne(ma => ma.Artist)
            .WithMany(a => a.Musics) // Alterado de .Musics para .MusicsArtists
            .HasForeignKey(ma => ma.ArtistsId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MusicsArtists>()
            .HasOne(ma => ma.Music)
            .WithMany(m => m.Artists)
            .HasForeignKey(ma => ma.MusicsId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MusicGenres>()
        .HasOne(mg => mg.Genre)
        .WithMany(g => g.Musics)
        .HasForeignKey(mg => mg.GenresId)
        .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MusicGenres>()
        .HasOne(m => m.Music)
        .WithMany(mg => mg.Genres)
        .HasForeignKey(m => m.MusicsId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}