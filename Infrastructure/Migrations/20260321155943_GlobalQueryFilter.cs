using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class GlobalQueryFilter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Memberships",
                keyColumns: new[] { "OrganizationId", "UserId" },
                keyValues: new object[] { new Guid("c8f2e6ab-9f34-4b97-8b7c-1a5e86d78e42"), new Guid("c8f2e5ab-9f34-4b97-8b7c-1a5e86c77e42") },
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 21, 15, 59, 40, 150, DateTimeKind.Utc).AddTicks(278));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Memberships",
                keyColumns: new[] { "OrganizationId", "UserId" },
                keyValues: new object[] { new Guid("c8f2e6ab-9f34-4b97-8b7c-1a5e86d78e42"), new Guid("c8f2e5ab-9f34-4b97-8b7c-1a5e86c77e42") },
                column: "CreatedAtUtc",
                value: new DateTime(2026, 3, 21, 13, 48, 36, 787, DateTimeKind.Utc).AddTicks(2891));
        }
    }
}
