using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlexPulse.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCaloriesPerMinute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "WorkoutLogs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<double>(
                name: "CaloriesPerMinute",
                table: "Exercises",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaloriesPerMinute",
                table: "Exercises");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "WorkoutLogs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
