using Microsoft.EntityFrameworkCore.Migrations;

namespace Courses.API.Migrations
{
    public partial class AddedFilepathindiscussionforum : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                schema: "Course",
                table: "DiscussionForum",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileType",
                schema: "Course",
                table: "DiscussionForum",
                nullable: true);

            
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
          

            migrationBuilder.DropColumn(
                name: "FilePath",
                schema: "Course",
                table: "DiscussionForum");

            migrationBuilder.DropColumn(
                name: "FileType",
                schema: "Course",
                table: "DiscussionForum");

          
        }
    }
}
