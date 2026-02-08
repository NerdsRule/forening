using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Organization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class TaskDepartment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TTaskDepartments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TaskId = table.Column<int>(type: "INTEGER", nullable: false),
                    DepartmentId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TTaskDepartments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TTaskDepartments_TDepartments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "TDepartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TTaskDepartments_TTasks_TaskId",
                        column: x => x.TaskId,
                        principalTable: "TTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TTasks_DepartmentId",
                table: "TTasks",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TTaskDepartments_DepartmentId",
                table: "TTaskDepartments",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TTaskDepartments_TaskId_DepartmentId",
                table: "TTaskDepartments",
                columns: new[] { "TaskId", "DepartmentId" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TTasks_TDepartments_DepartmentId",
                table: "TTasks",
                column: "DepartmentId",
                principalTable: "TDepartments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TTasks_TDepartments_DepartmentId",
                table: "TTasks");

            migrationBuilder.DropTable(
                name: "TTaskDepartments");

            migrationBuilder.DropIndex(
                name: "IX_TTasks_DepartmentId",
                table: "TTasks");
        }
    }
}
