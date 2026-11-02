
using Spotsom.Api.Endpoints;
using SpotSom.Api.Data;
using SpotSom.Api.Endpoints;
using Spotsom.Api.Services;
using SpotSom.Api.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidation();
builder.SeedingSqlite();


builder.Services.AddScoped<AudioStorageService>();
builder.Services.AddScoped<MusicService>();
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});
builder.Services.AddControllers(options =>
{

    options.Filters.Add(new IgnoreAntiforgeryTokenAttribute());
});

builder.Services.AddAntiforgery();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy => policy
        .AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();

app.UseStaticFiles();
app.UseCors("Frontend");
app.UseAntiforgery();
app.MapArtistEndpoints();
app.MapAlbumEndpoints();
app.MapGenreEndpoints();
app.MapMusicEndpoint();
app.MapPlaylistEndpoints();
app.MapUserEndpoints();

app.MigrationDb();

app.Run();
