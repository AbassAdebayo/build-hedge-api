using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HedgeEntityUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CreatedByUserId",
                table: "HedgeContracts",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Memberships",
                keyColumns: new[] { "OrganizationId", "UserId" },
                keyValues: new object[] { new Guid("c8f2e6ab-9f34-4b97-8b7c-1a5e86d78e42"), new Guid("c8f2e5ab-9f34-4b97-8b7c-1a5e86c77e42") },
                column: "CreatedAtUtc",
                value: new DateTime(2026, 2, 23, 19, 28, 32, 519, DateTimeKind.Utc).AddTicks(1119));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "HedgeContracts");

            migrationBuilder.UpdateData(
                table: "Memberships",
                keyColumns: new[] { "OrganizationId", "UserId" },
                keyValues: new object[] { new Guid("c8f2e6ab-9f34-4b97-8b7c-1a5e86d78e42"), new Guid("c8f2e5ab-9f34-4b97-8b7c-1a5e86c77e42") },
                column: "CreatedAtUtc",
                value: new DateTime(2026, 2, 21, 16, 53, 43, 759, DateTimeKind.Utc).AddTicks(7603));
        }
    }
}
