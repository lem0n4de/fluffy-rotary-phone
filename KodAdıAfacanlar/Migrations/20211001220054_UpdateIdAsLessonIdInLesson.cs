using Microsoft.EntityFrameworkCore.Migrations;

namespace KodAdıAfacanlar.Migrations
{
    public partial class UpdateIdAsLessonIdInLesson : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Lessons",
                newName: "LessonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LessonId",
                table: "Lessons",
                newName: "Id");
        }
    }
}
