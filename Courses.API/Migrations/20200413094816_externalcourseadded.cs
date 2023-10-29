using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class externalcourseadded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            migrationBuilder.AddColumn<string>(
                name: "CourseURL",
                schema: "Course",
                table: "Course",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalProvider",
                schema: "Course",
                table: "Course",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalProvider",
                schema: "Course",
                table: "Course",
                nullable: false,
                defaultValue: false);

            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.DropColumn(
                name: "CourseURL",
                schema: "Course",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "ExternalProvider",
                schema: "Course",
                table: "Course");

            migrationBuilder.DropColumn(
                name: "IsExternalProvider",
                schema: "Course",
                table: "Course");

           
        }
    }
}
