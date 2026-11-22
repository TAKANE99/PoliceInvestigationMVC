using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliceInvestigationMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestigationProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestigationProgressRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvestigationId = table.Column<int>(type: "int", nullable: false),
                    PoliceOfficerId = table.Column<int>(type: "int", nullable: false),
                    ActivityDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActivityType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DateRecorded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationProgressRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestigationProgressRecords_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestigationProgressRecords_PoliceOfficers_PoliceOfficerId",
                        column: x => x.PoliceOfficerId,
                        principalTable: "PoliceOfficers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationProgressRecords_InvestigationId",
                table: "InvestigationProgressRecords",
                column: "InvestigationId");

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationProgressRecords_PoliceOfficerId",
                table: "InvestigationProgressRecords",
                column: "PoliceOfficerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestigationProgressRecords");
        }
    }
}
