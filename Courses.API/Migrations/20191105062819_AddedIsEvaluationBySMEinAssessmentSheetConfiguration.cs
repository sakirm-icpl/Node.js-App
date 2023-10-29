using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class AddedIsEvaluationBySMEinAssessmentSheetConfiguration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEvaluationBySME",
                schema: "Course",
                table: "AssessmentSheetConfiguration",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEvaluationBySME",
                schema: "Course",
                table: "AssessmentSheetConfiguration");
        }
    }
}
