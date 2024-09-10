using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Migrations
{
    /// <inheritdoc />
    public partial class Upd4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("cb51bfd9-b00e-496b-a521-b8a42392e489"));

            migrationBuilder.AddColumn<string>(
                name: "CharName",
                table: "Games",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("d0bb745c-704c-4ec0-9a07-dc74860e6a80"), "Fantasy", "Фэнтези" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("d0bb745c-704c-4ec0-9a07-dc74860e6a80"));

            migrationBuilder.DropColumn(
                name: "CharName",
                table: "Games");

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("cb51bfd9-b00e-496b-a521-b8a42392e489"), "Fantasy", "Фэнтези" });
        }
    }
}
