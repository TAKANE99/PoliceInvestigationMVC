using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliceInvestigationMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseReviewAndInvestigationTeam : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "CaseReviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvestigationId = table.Column<int>(type: "int", nullable: false),
                    ReviewComments = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ReviewDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseReviews_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvestigationOfficers",
                columns: table => new
                {
                    InvestigationId = table.Column<int>(type: "int", nullable: false),
                    PoliceOfficerId = table.Column<int>(type: "int", nullable: false),
                    TeamRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationOfficers", x => new { x.InvestigationId, x.PoliceOfficerId });
                    table.ForeignKey(
                        name: "FK_InvestigationOfficers_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvestigationOfficers_PoliceOfficers_PoliceOfficerId",
                        column: x => x.PoliceOfficerId,
                        principalTable: "PoliceOfficers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Investigations_CaseNumber",
                table: "Investigations",
                column: "CaseNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseReviews_InvestigationId",
                table: "CaseReviews",
                column: "InvestigationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationOfficers_PoliceOfficerId",
                table: "InvestigationOfficers",
                column: "PoliceOfficerId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseReviews");

            migrationBuilder.DropTable(
                name: "InvestigationOfficers");

            migrationBuilder.DropIndex(
                name: "IX_Investigations_CaseNumber",
                table: "Investigations");
        }
    }
}
