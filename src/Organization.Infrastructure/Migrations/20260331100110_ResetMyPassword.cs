using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Organization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ResetMyPassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TResetPasswords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ResetRequestCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsResetMailBlocked = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    LastResetRequestAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ResetToken = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ResetTokenCreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ResetMailBlockedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TResetPasswords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TResetPasswords_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TResetPasswords_AppUserId",
                table: "TResetPasswords",
                column: "AppUserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TResetPasswords");
        }
    }
}
