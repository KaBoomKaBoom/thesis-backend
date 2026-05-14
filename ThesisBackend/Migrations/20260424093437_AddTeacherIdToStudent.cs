using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ThesisBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddTeacherIdToStudent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeacherId",
                table: "Users",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeacherId",
                table: "Users");
        }
    }
}
