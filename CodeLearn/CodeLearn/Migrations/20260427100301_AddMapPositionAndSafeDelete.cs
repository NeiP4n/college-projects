using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CodeLearn.Migrations
{
    /// <inheritdoc />
    public partial class AddMapPositionAndSafeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentBlocks_Lessons_LessonId",
                table: "ContentBlocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Modules_ModuleId",
                table: "Lessons");

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Modules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "MapX",
                table: "Modules",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MapY",
                table: "Modules",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                table: "Lessons",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "ContentBlocks",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlocks_Lessons_LessonId",
                table: "ContentBlocks",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Modules_ModuleId",
                table: "Lessons",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContentBlocks_Lessons_LessonId",
                table: "ContentBlocks");

            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Modules_ModuleId",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "MapX",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "MapY",
                table: "Modules");

            migrationBuilder.AlterColumn<int>(
                name: "ModuleId",
                table: "Lessons",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "LessonId",
                table: "ContentBlocks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContentBlocks_Lessons_LessonId",
                table: "ContentBlocks",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_Modules_ModuleId",
                table: "Lessons",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
