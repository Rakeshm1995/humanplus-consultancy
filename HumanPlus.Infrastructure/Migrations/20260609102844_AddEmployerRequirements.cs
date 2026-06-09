using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanPlus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployerRequirements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ApproximateHiringVolume",
                table: "Employers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ManpowerTypeRequired",
                table: "Employers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceLocations",
                table: "Employers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApproximateHiringVolume",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "ManpowerTypeRequired",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "ServiceLocations",
                table: "Employers");
        }
    }
}
