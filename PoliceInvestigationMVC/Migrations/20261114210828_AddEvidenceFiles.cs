using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliceInvestigationMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EvidenceFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EvidenceId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateUploaded = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EvidenceFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EvidenceFiles_EvidenceRecords_EvidenceId",
                        column: x => x.EvidenceId,
                        principalTable: "EvidenceRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceFiles_EvidenceId",
                table: "EvidenceFiles",
                column: "EvidenceId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EvidenceFiles");
        }
    }
}
