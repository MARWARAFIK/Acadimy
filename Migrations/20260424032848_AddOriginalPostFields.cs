using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acadimy.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalPostFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OriginalPostId",
                table: "CommunityPosts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "OriginalPostType",
                table: "CommunityPosts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalPostId",
                table: "CommunityPosts");

            migrationBuilder.DropColumn(
                name: "OriginalPostType",
                table: "CommunityPosts");
        }
    }
}
