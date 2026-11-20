using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EduApp.Web.Migrations
{
    /// <inheritdoc />
    public partial class RenameEnrolledAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ErolledAt",
                table: "Enrollments",
                newName: "EnrolledAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EnrolledAt",
                table: "Enrollments",
                newName: "ErolledAt");
        }
    }
}
