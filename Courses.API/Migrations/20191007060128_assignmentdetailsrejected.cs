using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class assignmentdetailsrejected : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignmentDetailsRejected",
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
                    CourseCode = table.Column<string>(maxLength: 250, nullable: true),
                    AssignmentName = table.Column<string>(maxLength: 250, nullable: true),
                    UserId = table.Column<string>(maxLength: 250, nullable: true),
                    Status = table.Column<string>(maxLength: 250, nullable: true),
                    Remark = table.Column<string>(maxLength: 250, nullable: true),
                    ErrorMessage = table.Column<string>(maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentDetailsRejected", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentDetailsRejected",
                schema: "Course");
        }
    }
}
