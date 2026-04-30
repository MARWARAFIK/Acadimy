using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acadimy.Migrations
{
    /// <inheritdoc />
    public partial class AddIsArchivedToStudentPost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "StudentPosts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "StudentPosts");
        }
    }
}
