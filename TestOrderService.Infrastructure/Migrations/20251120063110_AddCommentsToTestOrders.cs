using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestOrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCommentsToTestOrders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestOrders_PatientId_ReviewId_CreateById_RunById_UpdateById",
                schema: "public",
                table: "TestOrders");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RunAt",
                schema: "public",
                table: "TestOrders",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone");

            migrationBuilder.CreateIndex(
                name: "IX_TestOrders_PatientId_ReviewId_CreateById_UpdateById",
                schema: "public",
                table: "TestOrders",
                columns: new[] { "PatientId", "ReviewId", "CreateById", "UpdateById" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TestOrders_PatientId_ReviewId_CreateById_UpdateById",
                schema: "public",
                table: "TestOrders");

            migrationBuilder.AlterColumn<DateTime>(
                name: "RunAt",
                schema: "public",
                table: "TestOrders",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TestOrders_PatientId_ReviewId_CreateById_RunById_UpdateById",
                schema: "public",
                table: "TestOrders",
                columns: new[] { "PatientId", "ReviewId", "CreateById", "RunById", "UpdateById" });
        }
    }
}
