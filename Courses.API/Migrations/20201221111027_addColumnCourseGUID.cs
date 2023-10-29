using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class addColumnCourseGUID : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CourseGUID",
                schema: "Course",
                table: "CourseLog",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CourseGUID",
                schema: "Course",
                table: "CourseLog");
        }
    }
}
