using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Migrations
{
    /// <inheritdoc />
    public partial class Upd9 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Games_GameThemes_GameThemeId",
                table: "Games");

            migrationBuilder.DropIndex(
                name: "IX_Games_GameThemeId",
                table: "Games");

            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("47ed3eab-4e2a-4f7c-b654-27bf5d51009a"));

            migrationBuilder.DropColumn(
                name: "GameThemeId",
                table: "Games");

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("63fdc8a8-1ce4-4bf5-b02e-24e774bd7e7f"), "Fantasy", "Фэнтези" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("63fdc8a8-1ce4-4bf5-b02e-24e774bd7e7f"));

            migrationBuilder.AddColumn<Guid>(
                name: "GameThemeId",
                table: "Games",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("47ed3eab-4e2a-4f7c-b654-27bf5d51009a"), "Fantasy", "Фэнтези" });

            migrationBuilder.CreateIndex(
                name: "IX_Games_GameThemeId",
                table: "Games",
                column: "GameThemeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Games_GameThemes_GameThemeId",
                table: "Games",
                column: "GameThemeId",
                principalTable: "GameThemes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
