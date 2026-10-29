using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeLearn.Migrations
{
    /// <inheritdoc />
    public partial class AddMapObjectHeight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "MapObjects",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Height",
                table: "MapObjects");
        }
    }
}
