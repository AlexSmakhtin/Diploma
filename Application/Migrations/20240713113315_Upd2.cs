using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Application.Migrations
{
    /// <inheritdoc />
    public partial class Upd2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "GameThemes",
                newName: "NameRu");

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "GameThemes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[,]
                {
                    { new Guid("22d11a30-8495-4ba7-b136-ce4e46c3f1a4"), "Science fiction", "Научная фантастика" },
                    { new Guid("51af0884-a245-439d-88af-e2d08d1d76c2"), "Apocalypse", "Апокалипсис" },
                    { new Guid("b2b550ae-02b3-4a90-a9e9-deab09c73602"), "Fantasy", "Фэнтези" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("22d11a30-8495-4ba7-b136-ce4e46c3f1a4"));

            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("51af0884-a245-439d-88af-e2d08d1d76c2"));

            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("b2b550ae-02b3-4a90-a9e9-deab09c73602"));

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "GameThemes");

            migrationBuilder.RenameColumn(
                name: "NameRu",
                table: "GameThemes",
                newName: "Name");
        }
    }
}
