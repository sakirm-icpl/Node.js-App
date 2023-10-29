using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class addedIsManagerEvaluation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManagerEvaluation",
                schema: "Course",
                table: "Course",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ManagerEvaluationId",
                schema: "Course",
                table: "Course",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManagerEvaluation",
                schema: "Course",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "ManagerEvaluationId",
                schema: "Course",
                table: "Course");
        }
    }
}
