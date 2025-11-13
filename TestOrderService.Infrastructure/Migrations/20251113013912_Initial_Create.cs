using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestOrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Create : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "TestOrders",
                schema: "public",
                columns: table => new
                {
                    TestOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    ReviewId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReviewAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreateById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RunById = table.Column<Guid>(type: "uuid", nullable: false),
                    RunAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TestOrderDescription = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestOrders", x => x.TestOrderId);
                });

            migrationBuilder.CreateTable(
                name: "Comments",
                schema: "public",
                columns: table => new
                {
                    CommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreateById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TestOrderId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comments", x => x.CommentId);
                    table.ForeignKey(
                        name: "FK_Comments_TestOrders_TestOrderId",
                        column: x => x.TestOrderId,
                        principalSchema: "public",
                        principalTable: "TestOrders",
                        principalColumn: "TestOrderId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_CreateById_UpdateById_TestOrderId",
                schema: "public",
                table: "Comments",
                columns: new[] { "CreateById", "UpdateById", "TestOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_Comments_TestOrderId",
                schema: "public",
                table: "Comments",
                column: "TestOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_TestOrders_PatientId_ReviewId_CreateById_RunById_UpdateById",
                schema: "public",
                table: "TestOrders",
                columns: new[] { "PatientId", "ReviewId", "CreateById", "RunById", "UpdateById" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Comments",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TestOrders",
                schema: "public");
        }
    }
}
