using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Application.Migrations
{
    /// <inheritdoc />
    public partial class Upd3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GameState = table.Column<string>(type: "text", nullable: false),
                    GameThemeId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_GameThemes_GameThemeId",
                        column: x => x.GameThemeId,
                        principalTable: "GameThemes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Games_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Role = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    GameId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("cb51bfd9-b00e-496b-a521-b8a42392e489"), "Fantasy", "Фэнтези" });

            migrationBuilder.CreateIndex(
                name: "IX_Games_GameThemeId",
                table: "Games",
                column: "GameThemeId");

            migrationBuilder.CreateIndex(
                name: "IX_Games_UserId",
                table: "Games",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_GameId",
                table: "Messages",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("cb51bfd9-b00e-496b-a521-b8a42392e489"));

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
    }
}
