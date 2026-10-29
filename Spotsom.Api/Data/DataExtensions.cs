using SpotSom.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace SpotSom.Api.Data;

/// <summary>
/// Provides startup-time database initialization and migration helpers.
/// </summary>
public static class DataExtensions
{
    /// <summary>
    /// Applies pending EF Core migrations when the application starts.
    /// </summary>
    public static void MigrationDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider
                            .GetRequiredService<SpotsomContext>();
        dbContext.Database.Migrate();
    }

    /// <summary>
    /// Registers SQLite and seeds the default catalog records on first database creation.
    /// </summary>
    public static void AddGenreSqlite(this WebApplicationBuilder builder)
    {
        var connString = builder.Configuration.GetConnectionString("Spotsom");

        builder.Services.AddSqlite<SpotsomContext>(
            connString,
            optionsAction: options => options.UseSeeding((context, _) =>
            {
                if (!context.Set<Genre>().Any())
                {
                    context.Set<Genre>().AddRange(
                        new Genre { Name = "Unknown" },
                        new Genre { Name = "Rock" },
                        new Genre { Name = "Pop" },
                        new Genre { Name = "Pop Rock" },
                        new Genre { Name = "Chill" },
                        new Genre { Name = "MPB" },
                        new Genre { Name = "Rap" },
                        new Genre { Name = "Trap" }
                    );

                    context.SaveChanges();
                }
                if (!context.Set<User>().Any())
                {
                    context.Set<User>().AddRange(
                        new User { Name = "Unknown" },
                        new User { Name = "Admin" }
                    );
                    context.SaveChanges();
                }

                if (!context.Set<Artist>().Any())
                {
                    context.Set<Artist>().AddRange(
                        new Artist { Name = "Unknown" }
                    );
                    context.SaveChanges();
                }

                if (!context.Set<Album>().Any())
                {
                    context.Set<Album>().AddRange(
                        new Album
                        {
                            Name = "Unknown",
                            ReleaseDate = DateOnly.FromDateTime(DateTime.Now),
                            ArtistId = 1,
                            Artist = context.Set<Artist>().FirstOrDefault(a => a.Id == 1)
                        }
                    );
                    context.SaveChanges();
                }
            }));

    }
}
