using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddWarehouseToOrderItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "WarehouseId",
                table: "OrderItems",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_OrderItems_WarehouseId",
                table: "OrderItems",
                column: "WarehouseId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Warehouses_WarehouseId",
                table: "OrderItems",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Warehouses_WarehouseId",
                table: "OrderItems");

            migrationBuilder.DropIndex(
                name: "IX_OrderItems_WarehouseId",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "WarehouseId",
                table: "OrderItems");
        }
    }
}
