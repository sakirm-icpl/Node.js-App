using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class isretrainingadded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRetraining",
                schema: "Course",
                table: "Course",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "noOfDays",
                schema: "Course",
                table: "Course",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRetraining",
                schema: "Course",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "noOfDays",
                schema: "Course",
                table: "Course");
        }
    }
}
