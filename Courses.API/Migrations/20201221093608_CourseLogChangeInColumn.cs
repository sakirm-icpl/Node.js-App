using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class CourseLogChangeInColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Course",
                table: "CourseLog");

            migrationBuilder.DropColumn(
                name: "IsActive",
                schema: "Course",
                table: "CourseLog");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Course",
                table: "CourseLog");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "Course",
                table: "CourseLog");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                schema: "Course",
                table: "CourseLog");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "Course",
                table: "CourseLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                schema: "Course",
                table: "CourseLog",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Course",
                table: "CourseLog",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                schema: "Course",
                table: "CourseLog",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                schema: "Course",
                table: "CourseLog",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
