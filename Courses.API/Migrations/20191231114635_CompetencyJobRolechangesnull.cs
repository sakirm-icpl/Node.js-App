using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class CompetencyJobRolechangesnull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RoleColumn2value",
                schema: "Course",
                table: "CompetencyJobRole",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<int>(
                name: "RoleColumn1value",
                schema: "Course",
                table: "CompetencyJobRole",
                nullable: true,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "RoleColumn2value",
                schema: "Course",
                table: "CompetencyJobRole",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RoleColumn1value",
                schema: "Course",
                table: "CompetencyJobRole",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
