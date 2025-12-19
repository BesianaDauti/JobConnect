using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobConnect.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployerBillingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSubscriptionActive",
                table: "Employers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidUntil",
                table: "Employers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubscriptionCanceledAt",
                table: "Employers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSubscriptionActive",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "PaidUntil",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "SubscriptionCanceledAt",
                table: "Employers");
        }
    }
}
