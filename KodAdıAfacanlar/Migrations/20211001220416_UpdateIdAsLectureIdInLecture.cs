using Microsoft.EntityFrameworkCore.Migrations;

namespace KodAdıAfacanlar.Migrations
{
    public partial class UpdateIdAsLectureIdInLecture : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Lectures",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "Lectures");

            migrationBuilder.AddColumn<int>(
                name: "LectureId",
                table: "Lectures",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0)
                .Annotation("Sqlite:Autoincrement", true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lectures",
                table: "Lectures",
                column: "LectureId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Lectures",
                table: "Lectures");

            migrationBuilder.DropColumn(
                name: "LectureId",
                table: "Lectures");

            migrationBuilder.AddColumn<string>(
                name: "Id",
                table: "Lectures",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Lectures",
                table: "Lectures",
                column: "Id");
        }
    }
}
