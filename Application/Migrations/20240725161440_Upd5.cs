using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Migrations
{
    /// <inheritdoc />
    public partial class Upd5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("d0bb745c-704c-4ec0-9a07-dc74860e6a80"));

            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "Games",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("fbd13ff3-d951-4d8a-9d27-7a7b7bbce64c"), "Fantasy", "Фэнтези" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("fbd13ff3-d951-4d8a-9d27-7a7b7bbce64c"));

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "Games");

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("d0bb745c-704c-4ec0-9a07-dc74860e6a80"), "Fantasy", "Фэнтези" });
        }
    }
}
