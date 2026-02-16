using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiyaslasana.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddTelefonReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "telefon_reviews",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    telefon_slug = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    excerpt = table.Column<string>(type: "nvarchar(900)", maxLength: 900, nullable: true),
                    raw_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sanitized_content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    seo_title = table.Column<string>(type: "nvarchar(260)", maxLength: 260, nullable: true),
                    seo_description = table.Column<string>(type: "nvarchar(350)", maxLength: 350, nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_telefon_reviews", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_telefon_reviews_telefon_slug",
                table: "telefon_reviews",
                column: "telefon_slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_telefon_reviews_updated_at",
                table: "telefon_reviews",
                column: "updated_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "telefon_reviews");
        }
    }
}
