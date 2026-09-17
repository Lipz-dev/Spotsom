using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spotsom.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<TimeOnly>(
                name: "Duration",
                table: "Playlists",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReleaseDate",
                table: "Playlists",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<TimeSpan>(
                name: "Duration",
                table: "Musics",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<DateOnly>(
                name: "ReleaseDate",
                table: "Musics",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "Musics");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                table: "Musics");
        }
    }
}
