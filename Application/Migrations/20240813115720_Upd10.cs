using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Migrations
{
    /// <inheritdoc />
    public partial class Upd10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("63fdc8a8-1ce4-4bf5-b02e-24e774bd7e7f"));

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Games",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("3ab2c42d-6d92-4959-b60d-11469b40e54d"), "Fantasy", "Фэнтези" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("3ab2c42d-6d92-4959-b60d-11469b40e54d"));

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Games");

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("63fdc8a8-1ce4-4bf5-b02e-24e774bd7e7f"), "Fantasy", "Фэнтези" });
        }
    }
}
