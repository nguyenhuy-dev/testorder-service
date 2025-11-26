using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestOrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestResultsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestResults",
                schema: "public",
                columns: table => new
                {
                    TestResultId = table.Column<Guid>(type: "uuid", nullable: false),
                    Value = table.Column<double>(type: "double precision", nullable: false),
                    Flag = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    TestDefinitionId = table.Column<int>(type: "integer", nullable: false),
                    TestOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReviewedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TestResultDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResults", x => x.TestResultId);
                    table.ForeignKey(
                        name: "FK_TestResults_TestOrders_TestOrderId",
                        column: x => x.TestOrderId,
                        principalSchema: "public",
                        principalTable: "TestOrders",
                        principalColumn: "TestOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestResults_TestOrderId",
                schema: "public",
                table: "TestResults",
                column: "TestOrderId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestResults",
                schema: "public");
        }
    }
}
