using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddInventoryReservationAndRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservedQuantity",
                table: "InventoryBalances",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "InventoryBalances",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryBalances_QuantityOnHand",
                table: "InventoryBalances",
                sql: "[QuantityOnHand] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_InventoryBalances_ReservedQuantity",
                table: "InventoryBalances",
                sql: "[ReservedQuantity] >= 0 AND [ReservedQuantity] <= [QuantityOnHand]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryBalances_QuantityOnHand",
                table: "InventoryBalances");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InventoryBalances_ReservedQuantity",
                table: "InventoryBalances");

            migrationBuilder.DropColumn(
                name: "ReservedQuantity",
                table: "InventoryBalances");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "InventoryBalances");
        }
    }
}
