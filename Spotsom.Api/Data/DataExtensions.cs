using SpotSom.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace SpotSom.Api.Data;

public static class DataExtensions
{
    public static void MigrationDb(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider
                            .GetRequiredService<SpotsomContext>();
        dbContext.Database.Migrate();
    }

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
            }));

    }
}
