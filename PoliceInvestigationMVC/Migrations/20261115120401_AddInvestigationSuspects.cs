using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliceInvestigationMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddInvestigationSuspects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InvestigationSuspects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvestigationId = table.Column<int>(type: "int", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IDNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhysicalDescription = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IdentificationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArrestStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentificationMethod = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateRecorded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvestigationSuspects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvestigationSuspects_Investigations_InvestigationId",
                        column: x => x.InvestigationId,
                        principalTable: "Investigations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvestigationSuspects_InvestigationId",
                table: "InvestigationSuspects",
                column: "InvestigationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvestigationSuspects");
        }
    }
}
