using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HumanPlus.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruiterAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecruiterAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobDemandId = table.Column<int>(type: "int", nullable: false),
                    RecruiterUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AssignedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruiterAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecruiterAssignment_AspNetUsers_RecruiterUserId",
                        column: x => x.RecruiterUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruiterAssignment_JobDemands_JobDemandId",
                        column: x => x.JobDemandId,
                        principalTable: "JobDemands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterAssignment_JobDemandId",
                table: "RecruiterAssignment",
                column: "JobDemandId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruiterAssignment_RecruiterUserId",
                table: "RecruiterAssignment",
                column: "RecruiterUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecruiterAssignment");
        }
    }
}
