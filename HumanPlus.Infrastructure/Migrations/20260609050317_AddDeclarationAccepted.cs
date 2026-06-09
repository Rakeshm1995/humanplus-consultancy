using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanPlus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeclarationAccepted : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeclarationAccepted",
                table: "Candidates",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeclarationAccepted",
                table: "Candidates");
        }
    }
}
