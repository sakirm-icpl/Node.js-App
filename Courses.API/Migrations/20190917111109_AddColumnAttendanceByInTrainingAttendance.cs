using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class AddColumnAttendanceByInTrainingAttendance : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AttendanceBy",
                schema: "Course",
                table: "ILTTrainingAttendanceDetails",
                nullable: false,
                defaultValue: 0);      
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendanceBy",
                schema: "Course",
                table: "ILTTrainingAttendanceDetails");

        }
    }
}
