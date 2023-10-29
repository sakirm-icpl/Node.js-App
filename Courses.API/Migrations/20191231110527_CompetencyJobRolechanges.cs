using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class CompetencyJobRolechanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RoleColumn1",
                schema: "Course",
                table: "CompetencyJobRole",
                nullable: true,
                oldClrType: typeof(string));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "RoleColumn1",
                schema: "Course",
                table: "CompetencyJobRole",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
