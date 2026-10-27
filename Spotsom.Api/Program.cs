
using Spotsom.Api.Endpoints;
using SpotSom.Api.Data;
using SpotSom.Api.Endpoints;
using System.Net;
using Spotsom.Api.Services;
using SpotSom.Api.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidation();
builder.AddGenreSqlite();


builder.Services.AddScoped<AudioStorageService>();
builder.Services.AddScoped<MusicService>();
builder.WebHost.ConfigureKestrel(options =>
{
    options.Listen(IPAddress.Loopback, 5000);
});
builder.Services.AddControllers(options =>
{

    options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
});

builder.Services.AddAntiforgery();

var connString = "Data Source=Spotsom.db";
builder.Services.AddSqlite<SpotsomContext>(connString);

var app = builder.Build();

app.UseAntiforgery();
app.MapArtistEndpoints();
app.MapAlbumEndpoints();
app.MapGenreEndpoints();
app.MapMusicEndpoint();
app.MapPlaylistEndpoints();
app.MapUserEndpoints();

app.MigrationDb();

app.Run();
