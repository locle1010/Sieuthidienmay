using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebsiteBanHang.Migrations
{
    /// <inheritdoc />
    public partial class AddStockEntryTablea : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockEntry_Products_ProductId",
                table: "StockEntry");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockEntry",
                table: "StockEntry");

            migrationBuilder.RenameTable(
                name: "StockEntry",
                newName: "StockEntries");

            migrationBuilder.RenameIndex(
                name: "IX_StockEntry_ProductId_CreatedDate",
                table: "StockEntries",
                newName: "IX_StockEntries_ProductId_CreatedDate");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockEntries",
                table: "StockEntries",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockEntries_Products_ProductId",
                table: "StockEntries",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StockEntries_Products_ProductId",
                table: "StockEntries");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockEntries",
                table: "StockEntries");

            migrationBuilder.RenameTable(
                name: "StockEntries",
                newName: "StockEntry");

            migrationBuilder.RenameIndex(
                name: "IX_StockEntries_ProductId_CreatedDate",
                table: "StockEntry",
                newName: "IX_StockEntry_ProductId_CreatedDate");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockEntry",
                table: "StockEntry",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockEntry_Products_ProductId",
                table: "StockEntry",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
