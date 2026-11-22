using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PoliceInvestigationMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddOfficerLoginAndUnassignedCases : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investigations_PoliceOfficers_InvestigatingOfficerId",
                table: "Investigations");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "PoliceOfficers",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "InvestigatingOfficerId",
                table: "Investigations",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_PoliceOfficers_ApplicationUserId",
                table: "PoliceOfficers",
                column: "ApplicationUserId",
                unique: true,
                filter: "[ApplicationUserId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Investigations_PoliceOfficers_InvestigatingOfficerId",
                table: "Investigations",
                column: "InvestigatingOfficerId",
                principalTable: "PoliceOfficers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliceOfficers_AspNetUsers_ApplicationUserId",
                table: "PoliceOfficers",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Investigations_PoliceOfficers_InvestigatingOfficerId",
                table: "Investigations");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliceOfficers_AspNetUsers_ApplicationUserId",
                table: "PoliceOfficers");

            migrationBuilder.DropIndex(
                name: "IX_PoliceOfficers_ApplicationUserId",
                table: "PoliceOfficers");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "PoliceOfficers");

            migrationBuilder.AlterColumn<int>(
                name: "InvestigatingOfficerId",
                table: "Investigations",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Investigations_PoliceOfficers_InvestigatingOfficerId",
                table: "Investigations",
                column: "InvestigatingOfficerId",
                principalTable: "PoliceOfficers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
