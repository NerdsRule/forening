using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Organization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFidoCredentials : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TFidoCredentials",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AppUserId = table.Column<string>(type: "TEXT", nullable: false),
                    CredentialId = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: false),
                    PublicKey = table.Column<string>(type: "TEXT", nullable: false),
                    UserHandle = table.Column<string>(type: "TEXT", nullable: false),
                    SignatureCounter = table.Column<uint>(type: "INTEGER", nullable: false),
                    CredentialType = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    AaGuid = table.Column<string>(type: "TEXT", nullable: true),
                    Transports = table.Column<string>(type: "TEXT", maxLength: 1024, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TFidoCredentials", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TFidoCredentials_AspNetUsers_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TFidoCredentials_AppUserId",
                table: "TFidoCredentials",
                column: "AppUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TFidoCredentials_CredentialId",
                table: "TFidoCredentials",
                column: "CredentialId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TFidoCredentials");
        }
    }
}
