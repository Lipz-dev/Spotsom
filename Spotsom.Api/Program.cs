
using SpotSom.Api.Data;
using SpotSom.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidation();
builder.AddGenreSqlite();
var connString = "Data Source=Spotsom.db";
builder.Services.AddSqlite<SpotsomContext>(connString);

var app = builder.Build();

app.MapMusicEndpoints();
app.MapPlaylistEndpoints();

app.MigrationDb();


app.Run();
