using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Booki.Migrations
{
    /// <inheritdoc />
    public partial class AddingBooksCommentary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Commentary",
                table: "Books",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Commentary",
                table: "Books");
        }
    }
}
