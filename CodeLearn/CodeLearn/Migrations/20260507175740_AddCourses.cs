using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CodeLearn.Migrations
{
    /// <inheritdoc />
    public partial class AddCourses : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Modules",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Track = table.Column<string>(type: "text", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<string>(type: "text", nullable: false),
                    Badge = table.Column<string>(type: "text", nullable: false),
                    Categories = table.Column<string>(type: "text", nullable: false),
                    EstimatedHours = table.Column<int>(type: "integer", nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsPublished = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_CourseId",
                table: "Modules",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Slug",
                table: "Courses",
                column: "Slug",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_Courses_CourseId",
                table: "Modules",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.Sql("""
                INSERT INTO "Courses" ("Title", "Slug", "Description", "Track", "Tag", "Color", "State", "Badge", "Categories", "EstimatedHours", "Order", "IsPublished")
                SELECT 'C# / .NET', 'csharp', 'Основной курс CodeLearn', 'BACKEND', 'основы - oop - asp.net', '#8f7cff', 'available', 'START', 'backend', 6, 0, TRUE
                WHERE NOT EXISTS (SELECT 1 FROM "Courses" WHERE "Slug" = 'csharp');

                UPDATE "Modules"
                SET "CourseId" = (SELECT "Id" FROM "Courses" WHERE "Slug" = 'csharp' LIMIT 1)
                WHERE "CourseId" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_Courses_CourseId",
                table: "Modules");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropIndex(
                name: "IX_Modules_CourseId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Modules");
        }
    }
}
