using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class nextJobRoleIDAddedd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.AddColumn<int>(
                name: "NextJobRoleId",
                schema: "Course",
                table: "NextJobRoles",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.DropColumn(
                name: "NextJobRoleId",
                schema: "Course",
                table: "NextJobRoles");
        }
    }
}
