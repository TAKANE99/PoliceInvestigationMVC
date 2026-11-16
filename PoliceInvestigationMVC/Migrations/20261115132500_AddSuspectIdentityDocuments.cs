using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliceInvestigationMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddSuspectIdentityDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IDCopyFileName",
                table: "InvestigationSuspects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDCopyFilePath",
                table: "InvestigationSuspects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDPhotoFileName",
                table: "InvestigationSuspects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IDPhotoFilePath",
                table: "InvestigationSuspects",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IDCopyFileName",
                table: "InvestigationSuspects");

            migrationBuilder.DropColumn(
                name: "IDCopyFilePath",
                table: "InvestigationSuspects");

            migrationBuilder.DropColumn(
                name: "IDPhotoFileName",
                table: "InvestigationSuspects");

            migrationBuilder.DropColumn(
                name: "IDPhotoFilePath",
                table: "InvestigationSuspects");
        }
    }
}
