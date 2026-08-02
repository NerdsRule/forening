using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Organization.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MultiRoleMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TAppUserDepartmentRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserDepartmentId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAppUserDepartmentRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TAppUserDepartmentRoles_TAppUserDepartments_AppUserDepartmentId",
                        column: x => x.AppUserDepartmentId,
                        principalTable: "TAppUserDepartments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TAppUserOrganizationRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppUserOrganizationId = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TAppUserOrganizationRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TAppUserOrganizationRoles_TAppUserOrganizations_AppUserOrganizationId",
                        column: x => x.AppUserOrganizationId,
                        principalTable: "TAppUserOrganizations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TAppUserDepartmentRoles_AppUserDepartmentId_Role",
                table: "TAppUserDepartmentRoles",
                columns: new[] { "AppUserDepartmentId", "Role" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TAppUserOrganizationRoles_AppUserOrganizationId_Role",
                table: "TAppUserOrganizationRoles",
                columns: new[] { "AppUserOrganizationId", "Role" },
                unique: true);

            migrationBuilder.Sql("""
                INSERT INTO TAppUserOrganizationRoles (AppUserOrganizationId, Role)
                SELECT Id, Role
                FROM TAppUserOrganizations;
            """);

            migrationBuilder.Sql("""
                INSERT INTO TAppUserDepartmentRoles (AppUserDepartmentId, Role)
                SELECT Id, Role
                FROM TAppUserDepartments;
            """);

            migrationBuilder.DropColumn(
                name: "Role",
                table: "TAppUserOrganizations");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "TAppUserDepartments");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "TAppUserOrganizations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "TAppUserDepartments",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                UPDATE uo
                SET uo.Role = src.Role
                FROM TAppUserOrganizations uo
                INNER JOIN (
                    SELECT AppUserOrganizationId, MIN(Role) AS Role
                    FROM TAppUserOrganizationRoles
                    GROUP BY AppUserOrganizationId
                ) src ON src.AppUserOrganizationId = uo.Id;
            """);

            migrationBuilder.Sql("""
                UPDATE ud
                SET ud.Role = src.Role
                FROM TAppUserDepartments ud
                INNER JOIN (
                    SELECT AppUserDepartmentId, MIN(Role) AS Role
                    FROM TAppUserDepartmentRoles
                    GROUP BY AppUserDepartmentId
                ) src ON src.AppUserDepartmentId = ud.Id;
            """);

            migrationBuilder.DropTable(
                name: "TAppUserDepartmentRoles");

            migrationBuilder.DropTable(
                name: "TAppUserOrganizationRoles");
        }
    }
}
