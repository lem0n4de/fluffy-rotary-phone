using Microsoft.EntityFrameworkCore.Migrations;

namespace KodAdıAfacanlar.Migrations
{
    public partial class LessonIdToLecture : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_Lessons_LessonId",
                table: "Lectures");

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "Lectures",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_Lessons_LessonId",
                table: "Lectures",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Lectures_Lessons_LessonId",
                table: "Lectures");

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "Lectures",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddForeignKey(
                name: "FK_Lectures_Lessons_LessonId",
                table: "Lectures",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
