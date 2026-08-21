using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Korp.StockService.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestHashToStockOperations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RequestHash",
                table: "stock_operations",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RequestHash",
                table: "stock_operations");
        }
    }
}
