using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobConnect.Migrations
{
    /// <inheritdoc />
    public partial class AddStripeFieldsToEmployer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Plan",
                table: "Employers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "StripeCustomerId",
                table: "Employers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StripeSubscriptionId",
                table: "Employers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Plan",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "StripeCustomerId",
                table: "Employers");

            migrationBuilder.DropColumn(
                name: "StripeSubscriptionId",
                table: "Employers");
        }
    }
}
