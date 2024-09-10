using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Migrations
{
    /// <inheritdoc />
    public partial class Upd6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("fbd13ff3-d951-4d8a-9d27-7a7b7bbce64c"));

            migrationBuilder.RenameColumn(
                name: "AvatarUrl",
                table: "Games",
                newName: "AvatarId");

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("26cf7c7a-1dde-4ce9-87ed-4bc96efa9150"), "Fantasy", "Фэнтези" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("26cf7c7a-1dde-4ce9-87ed-4bc96efa9150"));

            migrationBuilder.RenameColumn(
                name: "AvatarId",
                table: "Games",
                newName: "AvatarUrl");

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("fbd13ff3-d951-4d8a-9d27-7a7b7bbce64c"), "Fantasy", "Фэнтези" });
        }
    }
}
