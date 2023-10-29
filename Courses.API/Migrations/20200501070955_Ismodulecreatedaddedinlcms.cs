using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class Ismodulecreatedaddedinlcms : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ismodulecreate",
                schema: "Course",
                table: "LCMS",
                nullable: false,
                defaultValue: false);

                  }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ismodulecreate",
                schema: "Course",
                table: "LCMS");

                   }
    }
}
