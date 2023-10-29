using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class ProcessResultAndProcessResultDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessResult",
                schema: "Course",
                columns: table => new
                {
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
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
                    CreatedBy = table.Column<int>(nullable: false),
                    ModifiedBy = table.Column<int>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    IsDeleted = table.Column<bool>(nullable: false),
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    EvalResultID = table.Column<int>(nullable: false),
                    QuestionID = table.Column<int>(nullable: false),
                    OptionAnswerId = table.Column<int>(nullable: true),
                    SelectedAnswer = table.Column<string>(maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessResultDetails", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessResult",
                schema: "Course");
        
            migrationBuilder.DropTable(
                name: "ProcessResultDetails",
                schema: "Course");
        }
    }
}
