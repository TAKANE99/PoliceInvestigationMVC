using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliceInvestigationMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddPoliceOfficerDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "PoliceOfficers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "PoliceOfficers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmploymentStatus",
                table: "PoliceOfficers",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServiceNumber",
                table: "PoliceOfficers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // Give existing officers unique service numbers.
            migrationBuilder.Sql(
                @"UPDATE PoliceOfficers
          SET ServiceNumber = 'LEGACY-' + CAST(Id AS NVARCHAR(13))
          WHERE ServiceNumber = '';");

            // Set existing officers to Active.
            migrationBuilder.Sql(
                @"UPDATE PoliceOfficers
          SET EmploymentStatus = 'Active'
          WHERE EmploymentStatus = '';");

            // Create the unique index AFTER assigning service numbers.
            migrationBuilder.CreateIndex(
                name: "IX_PoliceOfficers_ServiceNumber",
                table: "PoliceOfficers",
                column: "ServiceNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PoliceOfficers_ServiceNumber",
                table: "PoliceOfficers");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "PoliceOfficers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "PoliceOfficers");

            migrationBuilder.DropColumn(
                name: "EmploymentStatus",
                table: "PoliceOfficers");

            migrationBuilder.DropColumn(
                name: "ServiceNumber",
                table: "PoliceOfficers");
        }
    }
}
