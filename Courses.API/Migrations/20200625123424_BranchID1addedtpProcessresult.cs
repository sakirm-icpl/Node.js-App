using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class BranchID1addedtpProcessresult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BranchId",
                schema: "Course",
                table: "ProcessResult",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BranchId",
                schema: "Course",
                table: "ProcessResult");
        }
    }
}
