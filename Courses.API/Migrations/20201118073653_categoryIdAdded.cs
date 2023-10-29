using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class categoryIdAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           
            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                schema: "Course",
                table: "AccessibilityRule",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                schema: "Course",
                table: "AccessibilityRule",
                nullable: true);

          
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
          
            migrationBuilder.DropColumn(
                name: "CategoryId",
                schema: "Course",
                table: "AccessibilityRule");

            migrationBuilder.AlterColumn<int>(
                name: "CourseId",
                schema: "Course",
                table: "AccessibilityRule",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
