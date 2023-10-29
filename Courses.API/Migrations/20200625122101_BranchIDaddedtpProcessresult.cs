using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class BranchIDaddedtpProcessresult : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QuestionText",
                schema: "Course",
                table: "AssessmentQuestionRejected",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CourseCertificateAuthority",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false),
                    CourseId = table.Column<int>(nullable: false),
                    UserID = table.Column<int>(nullable: false),
                    DesignationID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCertificateAuthority", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessConfiguration",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ProcessManagementId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessConfiguration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessEvaluationManagement",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Title = table.Column<string>(nullable: true),
                    Objective = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessEvaluationManagement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessEvaluationOption",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    QuestionID = table.Column<int>(nullable: false),
                    OptionText = table.Column<string>(maxLength: 500, nullable: true),
                    IsCorrectAnswer = table.Column<bool>(nullable: false),
                    RefQuestionID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessEvaluationOption", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessEvaluationQuestion",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    OptionType = table.Column<string>(maxLength: 50, nullable: false),
                    Section = table.Column<string>(maxLength: 50, nullable: true),
                    Category = table.Column<string>(maxLength: 50, nullable: true),
                    QuestionText = table.Column<string>(maxLength: 1000, nullable: false),
                    Marks = table.Column<int>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    AllowNA = table.Column<bool>(nullable: false),
                    IsSubquestion = table.Column<bool>(nullable: false),
                    IsRequired = table.Column<bool>(nullable: false),
                    Metadata = table.Column<string>(maxLength: 200, nullable: true),
                    AllowTextReply = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessEvaluationQuestion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessResult",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    ManagementId = table.Column<int>(nullable: false),
                    TransactionId = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: true),
                    Result = table.Column<string>(maxLength: 30, nullable: true),
                    MarksObtained = table.Column<double>(nullable: false),
                    Percentage = table.Column<decimal>(nullable: false),
                    NoOfAttempts = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessResult", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessResultDetails",
                schema: "Course",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    EvalResultID = table.Column<int>(nullable: false),
                    QuestionID = table.Column<int>(nullable: false),
                    OptionAnswerId = table.Column<int>(nullable: true),
                    SelectedAnswer = table.Column<string>(maxLength: 500, nullable: true),
                    ImprovementAnswer = table.Column<string>(maxLength: 500, nullable: true),
                    Marks = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessResultDetails", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseCertificateAuthority",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ProcessConfiguration",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ProcessEvaluationManagement",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ProcessEvaluationOption",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ProcessEvaluationQuestion",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ProcessResult",
                schema: "Course");

            migrationBuilder.DropTable(
                name: "ProcessResultDetails",
                schema: "Course");

            migrationBuilder.AlterColumn<string>(
                name: "QuestionText",
                schema: "Course",
                table: "AssessmentQuestionRejected",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 2000,
                oldNullable: true);
        }
    }
}
