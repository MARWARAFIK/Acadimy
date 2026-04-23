using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acadimy.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTeacherCoursesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "TeacherCourses");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TeacherCourses",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "TeacherCourses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Level",
                table: "TeacherCourses",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "TeacherCourses",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "TeacherCourses");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "TeacherCourses");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "TeacherCourses");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "TeacherCourses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "TeacherCourses",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
