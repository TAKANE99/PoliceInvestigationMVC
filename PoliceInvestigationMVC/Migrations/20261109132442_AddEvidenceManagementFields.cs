using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliceInvestigationMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddEvidenceManagementFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCollected",
                table: "EvidenceRecords",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChainOfCustody",
                table: "EvidenceRecords",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CollectedById",
                table: "EvidenceRecords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "EvidenceReference",
                table: "EvidenceRecords",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StorageLocation",
                table: "EvidenceRecords",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceRecords_CollectedById",
                table: "EvidenceRecords",
                column: "CollectedById");

            migrationBuilder.CreateIndex(
                name: "IX_EvidenceRecords_EvidenceReference",
                table: "EvidenceRecords",
                column: "EvidenceReference",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EvidenceRecords_PoliceOfficers_CollectedById",
                table: "EvidenceRecords",
                column: "CollectedById",
                principalTable: "PoliceOfficers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EvidenceRecords_PoliceOfficers_CollectedById",
                table: "EvidenceRecords");

            migrationBuilder.DropIndex(
                name: "IX_EvidenceRecords_CollectedById",
                table: "EvidenceRecords");

            migrationBuilder.DropIndex(
                name: "IX_EvidenceRecords_EvidenceReference",
                table: "EvidenceRecords");

            migrationBuilder.DropColumn(
                name: "ChainOfCustody",
                table: "EvidenceRecords");

            migrationBuilder.DropColumn(
                name: "CollectedById",
                table: "EvidenceRecords");

            migrationBuilder.DropColumn(
                name: "EvidenceReference",
                table: "EvidenceRecords");

            migrationBuilder.DropColumn(
                name: "StorageLocation",
                table: "EvidenceRecords");

            migrationBuilder.AlterColumn<DateTime>(
                name: "DateCollected",
                table: "EvidenceRecords",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
        }
    }
}
