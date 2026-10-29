using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeLearn.Migrations
{
    /// <inheritdoc />
    public partial class AddMapObjectInvert : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Invert",
                table: "MapObjects",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Invert",
                table: "MapObjects");
        }
    }
}
