using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TestOrderService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CreateByName",
                schema: "public",
                table: "Comments",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UpdateByName",
                schema: "public",
                table: "Comments",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreateByName",
                schema: "public",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "UpdateByName",
                schema: "public",
                table: "Comments");
        }
    }
}
