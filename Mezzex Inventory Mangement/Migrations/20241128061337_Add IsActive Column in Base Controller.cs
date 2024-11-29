using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mezzex_Inventory_Mangement.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveColumninBaseController : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "UserCompanyAssignments",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SellingChannel",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "ManageCompany",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Categories",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "UserCompanyAssignments");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SellingChannel");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "ManageCompany");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Categories");
        }
    }
}
