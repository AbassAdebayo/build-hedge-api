using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLastUpdatedByToprojectEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "UserRoles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "Roles",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "Projects",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "Memberships",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "Materials",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "MaterialPriceHistories",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "HedgeContracts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "DomainRules",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "DomainRules",
                keyColumn: "Id",
                keyValue: new Guid("6e3d4962-dcb0-42bc-9c58-7f6209d4a871"),
                column: "LastUpdatedBy",
                value: null);

            migrationBuilder.UpdateData(
                table: "DomainRules",
                keyColumn: "Id",
                keyValue: new Guid("6e3d4978-dcb0-42ea-9c48-7f6209e5b871"),
                column: "LastUpdatedBy",
                value: null);

            migrationBuilder.UpdateData(
                table: "DomainRules",
                keyColumn: "Id",
                keyValue: new Guid("6e3d4978-dcb0-42ea-9c48-7f6521d4a871"),
                column: "LastUpdatedBy",
                value: null);

            migrationBuilder.UpdateData(
                table: "DomainRules",
                keyColumn: "Id",
                keyValue: new Guid("6e3e8978-dcb0-42ea-9c78-7f6209d4a871"),
                column: "LastUpdatedBy",
                value: null);

            migrationBuilder.UpdateData(
                table: "DomainRules",
                keyColumn: "Id",
                keyValue: new Guid("9f3d4978-dcb0-42ea-9c48-7f8509d4a871"),
                column: "LastUpdatedBy",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6e3d4978-dcb0-42ea-9c48-7f6209d4a871"),
                column: "LastUpdatedBy",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("6e3d4978-dcb0-42ea-9c48-7f6498d4a871"),
                column: "LastUpdatedBy",
                value: null);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("a45c9e02-1f0b-4e57-b3d8-9b77b4a302be"),
                column: "LastUpdatedBy",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "UserRoles");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "Memberships");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "MaterialPriceHistories");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "HedgeContracts");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "DomainRules");
        }
    }
}
