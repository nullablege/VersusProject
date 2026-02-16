using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiyaslasana.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddBlogPosts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "blog_posts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    title = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    slug = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    excerpt = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    content_raw = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    content_sanitized = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    meta_title = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    meta_description = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    is_published = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_blog_posts", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_blog_posts_published_at",
                table: "blog_posts",
                column: "published_at");

            migrationBuilder.CreateIndex(
                name: "ix_blog_posts_slug",
                table: "blog_posts",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "blog_posts");
        }
    }
}
