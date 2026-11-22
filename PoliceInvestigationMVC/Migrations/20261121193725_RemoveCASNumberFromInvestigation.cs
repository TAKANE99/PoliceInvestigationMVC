using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliceInvestigationMVC.Migrations
{
    /// <inheritdoc />
    public partial class RemoveCASNumberFromInvestigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CASNumber",
                table: "Investigations");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CASNumber",
                table: "Investigations",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");
        }
    }
}
