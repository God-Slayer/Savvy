using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Savvy.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentRunStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "PaymentRuns",
                type: "int",
                nullable: false,
                defaultValue: 0
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "Status", table: "PaymentRuns");
        }
    }
}
