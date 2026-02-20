using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiyaslasana.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCompareVisits : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "compare_visits",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    slug_left = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    slug_right = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    visited_at = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ip_hash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_compare_visits", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_compare_visits_slug_left_slug_right_visited_at",
                table: "compare_visits",
                columns: new[] { "slug_left", "slug_right", "visited_at" });

            migrationBuilder.CreateIndex(
                name: "ix_compare_visits_visited_at",
                table: "compare_visits",
                column: "visited_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "compare_visits");
        }
    }
}
