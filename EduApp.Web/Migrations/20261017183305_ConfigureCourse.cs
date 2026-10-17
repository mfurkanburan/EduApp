using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lessons_CourseSectionId",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_CourseSections_CourseId",
                table: "CourseSections");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_CourseSectionId_Order",
                table: "Lessons",
                columns: new[] { "CourseSectionId", "Order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseSections_CourseId_Order",
                table: "CourseSections",
                columns: new[] { "CourseId", "Order" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Lessons_CourseSectionId_Order",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_CourseSections_CourseId_Order",
                table: "CourseSections");

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_CourseSectionId",
                table: "Lessons",
                column: "CourseSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSections_CourseId",
                table: "CourseSections",
                column: "CourseId");
        }
    }
}
