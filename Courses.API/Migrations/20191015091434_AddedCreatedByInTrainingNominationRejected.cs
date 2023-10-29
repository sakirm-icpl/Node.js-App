using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class AddedCreatedByInTrainingNominationRejected : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                schema: "Course",
                table: "TrainingNominationRejected",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedDate",
                schema: "Course",
                table: "TrainingNominationRejected",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "Course",
                table: "TrainingNominationRejected",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ModifiedBy",
                schema: "Course",
                table: "TrainingNominationRejected",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ModifiedDate",
                schema: "Course",
                table: "TrainingNominationRejected",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                schema: "Course",
                table: "TrainingNominationRejected");

            migrationBuilder.DropColumn(
                name: "CreatedDate",
                schema: "Course",
                table: "TrainingNominationRejected");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "Course",
                table: "TrainingNominationRejected");

            migrationBuilder.DropColumn(
                name: "ModifiedBy",
                schema: "Course",
                table: "TrainingNominationRejected");

            migrationBuilder.DropColumn(
                name: "ModifiedDate",
                schema: "Course",
                table: "TrainingNominationRejected");
        }
    }
}
