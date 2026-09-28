
using Spotsom.Api.Endpoints;
using SpotSom.Api.Data;
using SpotSom.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidation();
builder.AddGenreSqlite();
var connString = "Data Source=Spotsom.db";
builder.Services.AddSqlite<SpotsomContext>(connString);

var app = builder.Build();

app.MapArtistEndpoints();
app.MapAlbumEndpoints();
app.MapGenreEndpoints();
app.MapMusicEndpoint(); //!!NÃO CONSIGO CRIAR UMA MUSICA
app.MapPlaylistEndpoints();
app.MapUserEndpoints();

app.MigrationDb();


app.Run();
