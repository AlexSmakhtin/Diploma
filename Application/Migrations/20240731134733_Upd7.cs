using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Application.Migrations
{
    /// <inheritdoc />
    public partial class Upd7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("26cf7c7a-1dde-4ce9-87ed-4bc96efa9150"));

            migrationBuilder.CreateTable(
                name: "AiRecords",
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
                    table.PrimaryKey("PK_AiRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AiRecords_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("b5a8dfa0-67ee-4d50-b132-fbd8554c583d"), "Fantasy", "Фэнтези" });

            migrationBuilder.CreateIndex(
                name: "IX_AiRecords_GameId",
                table: "AiRecords",
                column: "GameId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiRecords");

            migrationBuilder.DeleteData(
                table: "GameThemes",
                keyColumn: "Id",
                keyValue: new Guid("b5a8dfa0-67ee-4d50-b132-fbd8554c583d"));

            migrationBuilder.InsertData(
                table: "GameThemes",
                columns: new[] { "Id", "NameEn", "NameRu" },
                values: new object[] { new Guid("26cf7c7a-1dde-4ce9-87ed-4bc96efa9150"), "Fantasy", "Фэнтези" });
        }
    }
}
