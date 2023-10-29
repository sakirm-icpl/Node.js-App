using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class AddedSequenceNoInSubCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SequenceNo",
                schema: "Course",
                table: "SubCategory",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SequenceNo",
                schema: "Course",
                table: "SubCategory");
        }
    }
}
