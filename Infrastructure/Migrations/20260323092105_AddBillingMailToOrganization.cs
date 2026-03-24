using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingMailToOrganization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillingEmail",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Memberships",
                keyColumns: new[] { "OrganizationId", "UserId" },
                keyValues: new object[] { new Guid("c8f2e6ab-9f34-4b97-8b7c-1a5e86d78e42"), new Guid("c8f2e5ab-9f34-4b97-8b7c-1a5e86c77e42") },
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 23, 9, 21, 3, 750, DateTimeKind.Utc).AddTicks(6945));

            migrationBuilder.UpdateData(
                table: "Organizations",
                keyColumn: "Id",
                keyValue: new Guid("c8f2e6ab-9f34-4b97-8b7c-1a5e86d78e42"),
                column: "BillingEmail",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillingEmail",
                table: "Organizations");

            migrationBuilder.UpdateData(
                table: "Memberships",
                keyColumns: new[] { "OrganizationId", "UserId" },
                keyValues: new object[] { new Guid("c8f2e6ab-9f34-4b97-8b7c-1a5e86d78e42"), new Guid("c8f2e5ab-9f34-4b97-8b7c-1a5e86c77e42") },
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 21, 15, 59, 40, 150, DateTimeKind.Utc).AddTicks(278));
        }
    }
}
