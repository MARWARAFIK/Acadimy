using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acadimy.Migrations
{
    /// <inheritdoc />
    public partial class FixCommentReply : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ParentCommentId",
                table: "TeacherPostComments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TeacherPostCommentLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TeacherPostCommentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeacherPostCommentLikes", x => x.Id);

                    table.ForeignKey(
                        name: "FK_TeacherPostCommentLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_TeacherPostCommentLikes_TeacherPostComments_TeacherPostCommentId",
                        column: x => x.TeacherPostCommentId,
                        principalTable: "TeacherPostComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TeacherPostComments_ParentCommentId",
                table: "TeacherPostComments",
                column: "ParentCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherPostCommentLikes_TeacherPostCommentId",
                table: "TeacherPostCommentLikes",
                column: "TeacherPostCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_TeacherPostCommentLikes_UserId",
                table: "TeacherPostCommentLikes",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TeacherPostComments_TeacherPostComments_ParentCommentId",
                table: "TeacherPostComments",
                column: "ParentCommentId",
                principalTable: "TeacherPostComments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TeacherPostComments_TeacherPostComments_ParentCommentId",
                table: "TeacherPostComments");

            migrationBuilder.DropTable(
                name: "TeacherPostCommentLikes");

            migrationBuilder.DropIndex(
                name: "IX_TeacherPostComments_ParentCommentId",
                table: "TeacherPostComments");

            migrationBuilder.DropColumn(
                name: "ParentCommentId",
                table: "TeacherPostComments");
        }
    }
}
