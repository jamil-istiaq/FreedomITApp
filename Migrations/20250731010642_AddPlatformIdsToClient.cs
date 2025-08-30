using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FreedomITAS.Migrations
{
    /// <inheritdoc />
    public partial class AddPlatformIdsToClient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyLegalName",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CompanyType",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DreamScapeId",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HaloId",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HuduId",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Pax8Id",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SyncroId",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZomentumId",
                table: "Clients",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyLegalName",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CompanyType",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "DreamScapeId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "HaloId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "HuduId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Pax8Id",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "SyncroId",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "Website",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "ZomentumId",
                table: "Clients");
        }
    }
}
