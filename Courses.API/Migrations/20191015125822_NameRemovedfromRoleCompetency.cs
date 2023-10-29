using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class NameRemovedfromRoleCompetency : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompetencyCategoryName",
                schema: "Course",
                table: "RoleCompetency");

            migrationBuilder.DropColumn(
                name: "CompetencyName",
                schema: "Course",
                table: "RoleCompetency");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompetencyCategoryName",
                schema: "Course",
                table: "RoleCompetency",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompetencyName",
                schema: "Course",
                table: "RoleCompetency",
                nullable: true);
        }
    }
}
