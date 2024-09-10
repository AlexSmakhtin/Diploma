using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Migrations
{
    /// <inheritdoc />
    public partial class Upd8 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("b5a8dfa0-67ee-4d50-b132-fbd8554c583d"));

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("47ed3eab-4e2a-4f7c-b654-27bf5d51009a"), "Fantasy", "Фэнтези" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("47ed3eab-4e2a-4f7c-b654-27bf5d51009a"));

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("b5a8dfa0-67ee-4d50-b132-fbd8554c583d"), "Fantasy", "Фэнтези" });
        }
    }
}
