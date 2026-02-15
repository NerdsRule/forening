using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Organization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaskPrizeStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TTasks",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "TPrizes",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "TTasks");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "TPrizes");
        }
    }
}
