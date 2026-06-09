using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanPlus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeEmployerIndustryIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employers_Industries_IndustryId",
                table: "Employers");

            migrationBuilder.AlterColumn<int>(
                name: "IndustryId",
                table: "Employers",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Employers_Industries_IndustryId",
                table: "Employers",
                column: "IndustryId",
                principalTable: "Industries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employers_Industries_IndustryId",
                table: "Employers");

            migrationBuilder.AlterColumn<int>(
                name: "IndustryId",
                table: "Employers",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Employers_Industries_IndustryId",
                table: "Employers",
                column: "IndustryId",
                principalTable: "Industries",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
