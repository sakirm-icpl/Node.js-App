using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class addrowguidinaccesibilityrule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UniqueMeetingId",
                schema: "Course",
                table: "ZoomMeetingDetails",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "RowGUID",
                schema: "Course",
                table: "AccessibilityRule",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowGUID",
                schema: "Course",
                table: "AccessibilityRule");

            migrationBuilder.AlterColumn<int>(
                name: "UniqueMeetingId",
                schema: "Course",
                table: "ZoomMeetingDetails",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldNullable: true);
        }
    }
}
