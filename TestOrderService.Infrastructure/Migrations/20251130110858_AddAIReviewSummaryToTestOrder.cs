using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestOrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAIReviewSummaryToTestOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AIReviewSummary",
                schema: "public",
                table: "TestOrders",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AIReviewSummary",
                schema: "public",
                table: "TestOrders");
        }
    }
}
