using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class addedIsManagerEvaluationtoassessmentresult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsManagerEvaluation",
                schema: "Course",
                table: "PostAssessmentResult",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsManagerEvaluation",
                schema: "Course",
                table: "PostAssessmentResult");
        }
    }
}
