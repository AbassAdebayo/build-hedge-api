using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class PaystackWebhook : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAtUtc",
                table: "ProcessedPayments",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "ProcessedPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "ProcessedPayments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ProcessedPayments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LastUpdatedBy",
                table: "ProcessedPayments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "ProcessedPayments",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAtUtc",
                table: "ProcessedPayments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "IsPaidAt",
                table: "BillingStatements",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.UpdateData(
                table: "Memberships",
                keyColumns: new[] { "OrganizationId", "UserId" },
                keyValues: new object[] { new Guid("c8f2e6ab-9f34-4b97-8b7c-1a5e86d78e42"), new Guid("c8f2e5ab-9f34-4b97-8b7c-1a5e86c77e42") },
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 21, 13, 48, 36, 787, DateTimeKind.Utc).AddTicks(2891));

            migrationBuilder.CreateIndex(
                name: "IX_BillingStatements_OrganizationId",
                table: "BillingStatements",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingStatements_Organizations_OrganizationId",
                table: "BillingStatements",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingStatements_Organizations_OrganizationId",
                table: "BillingStatements");

            migrationBuilder.DropIndex(
                name: "IX_BillingStatements_OrganizationId",
                table: "BillingStatements");

            migrationBuilder.DropColumn(
                name: "CreatedAtUtc",
                table: "ProcessedPayments");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "ProcessedPayments");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ProcessedPayments");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ProcessedPayments");

            migrationBuilder.DropColumn(
                name: "LastUpdatedBy",
                table: "ProcessedPayments");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "ProcessedPayments");

            migrationBuilder.DropColumn(
                name: "UpdatedAtUtc",
                table: "ProcessedPayments");

            migrationBuilder.DropColumn(
                name: "IsPaidAt",
                table: "BillingStatements");

            migrationBuilder.UpdateData(
                table: "Memberships",
                keyColumns: new[] { "OrganizationId", "UserId" },
                keyValues: new object[] { new Guid("c8f2e6ab-9f34-4b97-8b7c-1a5e86d78e42"), new Guid("c8f2e5ab-9f34-4b97-8b7c-1a5e86c77e42") },
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 16, 8, 44, 15, 519, DateTimeKind.Utc).AddTicks(9487));
        }
    }
}
