using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acadimy.Migrations
{
    /// <inheritdoc />
    public partial class AddStudentPostCommentLikes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CommunityCommentLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommunityCommentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommunityCommentLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CommunityCommentLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommunityCommentLikes_CommunityComments_CommunityCommentId",
                        column: x => x.CommunityCommentId,
                        principalTable: "CommunityComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentPostCommentLikes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentPostCommentId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPostCommentLikes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentPostCommentLikes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentPostCommentLikes_StudentPostComments_StudentPostCommentId",
                        column: x => x.StudentPostCommentId,
                        principalTable: "StudentPostComments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CommunityCommentLikes_CommunityCommentId",
                table: "CommunityCommentLikes",
                column: "CommunityCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommunityCommentLikes_UserId",
                table: "CommunityCommentLikes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPostCommentLikes_StudentPostCommentId",
                table: "StudentPostCommentLikes",
                column: "StudentPostCommentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPostCommentLikes_UserId",
                table: "StudentPostCommentLikes",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommunityCommentLikes");

            migrationBuilder.DropTable(
                name: "StudentPostCommentLikes");
        }
    }
}
