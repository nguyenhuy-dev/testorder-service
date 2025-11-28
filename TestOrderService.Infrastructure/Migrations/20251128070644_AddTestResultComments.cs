using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestOrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTestResultComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TestResultComments",
                schema: "public",
                columns: table => new
                {
                    TestResultCommentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreateById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateByName = table.Column<string>(type: "text", nullable: false),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdateById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdateByName = table.Column<string>(type: "text", nullable: false),
                    UpdateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TestResultId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TestResultComments", x => x.TestResultCommentId);
                    table.ForeignKey(
                        name: "FK_TestResultComments_TestResults_TestResultId",
                        column: x => x.TestResultId,
                        principalSchema: "public",
                        principalTable: "TestResults",
                        principalColumn: "TestResultId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TestResultComments_CreateById_UpdateById_TestResultId",
                schema: "public",
                table: "TestResultComments",
                columns: new[] { "CreateById", "UpdateById", "TestResultId" });

            migrationBuilder.CreateIndex(
                name: "IX_TestResultComments_TestResultId",
                schema: "public",
                table: "TestResultComments",
                column: "TestResultId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TestResultComments",
                schema: "public");
        }
    }
}
