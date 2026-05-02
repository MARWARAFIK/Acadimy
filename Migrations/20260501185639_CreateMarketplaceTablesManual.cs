using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acadimy.Migrations
{
    public partial class CreateMarketplaceTablesManual : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[ProjectPosts]', N'U') IS NULL
BEGIN
    CREATE TABLE [ProjectPosts] (
        [Id] int NOT NULL IDENTITY,
        [Title] nvarchar(200) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [ImagePath] nvarchar(max) NULL,
        [FilePath] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProjectPosts] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_ProjectPosts_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_ProjectPosts_UserId] ON [ProjectPosts] ([UserId]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[ProjectComments]', N'U') IS NULL
BEGIN
    CREATE TABLE [ProjectComments] (
        [Id] int NOT NULL IDENTITY,
        [ProjectPostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        CONSTRAINT [PK_ProjectComments] PRIMARY KEY ([Id]),

        CONSTRAINT [FK_ProjectComments_ProjectPosts_ProjectPostId]
            FOREIGN KEY ([ProjectPostId]) REFERENCES [ProjectPosts] ([Id]) ON DELETE CASCADE,

        CONSTRAINT [FK_ProjectComments_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_ProjectComments_ProjectPostId] ON [ProjectComments] ([ProjectPostId]);
    CREATE INDEX [IX_ProjectComments_UserId] ON [ProjectComments] ([UserId]);
END
");

            migrationBuilder.Sql(@"
IF OBJECT_ID(N'[ProjectRatings]', N'U') IS NULL
BEGIN
    CREATE TABLE [ProjectRatings] (
        [Id] int NOT NULL IDENTITY,
        [ProjectPostId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Value] int NOT NULL,
        CONSTRAINT [PK_ProjectRatings] PRIMARY KEY ([Id]),

        CONSTRAINT [FK_ProjectRatings_ProjectPosts_ProjectPostId]
            FOREIGN KEY ([ProjectPostId]) REFERENCES [ProjectPosts] ([Id]) ON DELETE CASCADE,

        CONSTRAINT [FK_ProjectRatings_AspNetUsers_UserId]
            FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION
    );

    CREATE INDEX [IX_ProjectRatings_ProjectPostId] ON [ProjectRatings] ([ProjectPostId]);
    CREATE INDEX [IX_ProjectRatings_UserId] ON [ProjectRatings] ([UserId]);
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"IF OBJECT_ID(N'[ProjectRatings]', N'U') IS NOT NULL DROP TABLE [ProjectRatings];");
            migrationBuilder.Sql(@"IF OBJECT_ID(N'[ProjectComments]', N'U') IS NOT NULL DROP TABLE [ProjectComments];");
            migrationBuilder.Sql(@"IF OBJECT_ID(N'[ProjectPosts]', N'U') IS NOT NULL DROP TABLE [ProjectPosts];");
        }
    }
}