using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class IsReviewedBySMEAddedInPostAssessmentResult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReviewedBySME",
                schema: "Course",
                table: "PostAssessmentResult",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReviewedBySME",
                schema: "Course",
                table: "PostAssessmentResult");
        }
    }
}
