using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kiyaslasana.DAL.Migrations
{
    /// <inheritdoc />
    public partial class HardenCompareVisitTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_compare_visits_slug_left_slug_right_visited_at",
                table: "compare_visits");

            migrationBuilder.RenameColumn(
                name: "slug_right",
                table: "compare_visits",
                newName: "canonical_right_slug");

            migrationBuilder.RenameColumn(
                name: "slug_left",
                table: "compare_visits",
                newName: "canonical_left_slug");

            migrationBuilder.CreateIndex(
                name: "ix_compare_visits_canonical_left_slug_canonical_right_slug",
                table: "compare_visits",
                columns: new[] { "canonical_left_slug", "canonical_right_slug" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_compare_visits_canonical_left_slug_canonical_right_slug",
                table: "compare_visits");

            migrationBuilder.RenameColumn(
                name: "canonical_right_slug",
                table: "compare_visits",
                newName: "slug_right");

            migrationBuilder.RenameColumn(
                name: "canonical_left_slug",
                table: "compare_visits",
                newName: "slug_left");

            migrationBuilder.CreateIndex(
                name: "ix_compare_visits_slug_left_slug_right_visited_at",
                table: "compare_visits",
                columns: new[] { "slug_left", "slug_right", "visited_at" });
        }
    }
}
