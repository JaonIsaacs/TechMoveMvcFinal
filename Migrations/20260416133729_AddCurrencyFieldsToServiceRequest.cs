using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TechMoveFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyFieldsToServiceRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Cost",
                table: "ServiceRequests",
                newName: "CostZar");

            migrationBuilder.AddColumn<decimal>(
                name: "CostUsd",
                table: "ServiceRequests",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRate",
                table: "ServiceRequests",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 0m);

            // Update existing records with estimated USD and exchange rate
            migrationBuilder.Sql(
                @"UPDATE ServiceRequests 
                  SET CostUsd = CostZar / 18.50, 
                      ExchangeRate = 18.50 
                  WHERE CostUsd = 0 AND CostZar > 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CostUsd",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ExchangeRate",
                table: "ServiceRequests");

            migrationBuilder.RenameColumn(
                name: "CostZar",
                table: "ServiceRequests",
                newName: "Cost");
        }
    }
}
