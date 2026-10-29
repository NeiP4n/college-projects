using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeLearn.Migrations
{
    /// <inheritdoc />
    public partial class AddIsChallenge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsChallenge",
                table: "Modules",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsChallenge",
                table: "Modules");
        }
    }
}
