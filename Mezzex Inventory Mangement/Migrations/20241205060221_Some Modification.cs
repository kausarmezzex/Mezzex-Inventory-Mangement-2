using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mezzex_Inventory_Mangement.Migrations
{
    /// <inheritdoc />
    public partial class SomeModification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BrandCategories_Brands_BrandsId",
                table: "BrandCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_BrandCategories_Categories_CategoriesId",
                table: "BrandCategories");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "UserCompanyAssignments",
                newName: "UserCompanyAssignmentId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Pages",
                newName: "PageId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "PageRoleMappings",
                newName: "PageRoleId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Categories",
                newName: "CategoryId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Brands",
                newName: "BrandId");

            migrationBuilder.RenameColumn(
                name: "CategoriesId",
                table: "BrandCategories",
                newName: "CategoriesCategoryId");

            migrationBuilder.RenameColumn(
                name: "BrandsId",
                table: "BrandCategories",
                newName: "BrandsBrandId");

            migrationBuilder.RenameIndex(
                name: "IX_BrandCategories_CategoriesId",
                table: "BrandCategories",
                newName: "IX_BrandCategories_CategoriesCategoryId");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "BlockedChannels",
                newName: "BlockChannelId");

            migrationBuilder.AddForeignKey(
                name: "FK_BrandCategories_Brands_BrandsBrandId",
                table: "BrandCategories",
                column: "BrandsBrandId",
                principalTable: "Brands",
                principalColumn: "BrandId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BrandCategories_Categories_CategoriesCategoryId",
                table: "BrandCategories",
                column: "CategoriesCategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BrandCategories_Brands_BrandsBrandId",
                table: "BrandCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_BrandCategories_Categories_CategoriesCategoryId",
                table: "BrandCategories");

            migrationBuilder.RenameColumn(
                name: "UserCompanyAssignmentId",
                table: "UserCompanyAssignments",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PageId",
                table: "Pages",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "PageRoleId",
                table: "PageRoleMappings",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Categories",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "BrandId",
                table: "Brands",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "CategoriesCategoryId",
                table: "BrandCategories",
                newName: "CategoriesId");

            migrationBuilder.RenameColumn(
                name: "BrandsBrandId",
                table: "BrandCategories",
                newName: "BrandsId");

            migrationBuilder.RenameIndex(
                name: "IX_BrandCategories_CategoriesCategoryId",
                table: "BrandCategories",
                newName: "IX_BrandCategories_CategoriesId");

            migrationBuilder.RenameColumn(
                name: "BlockChannelId",
                table: "BlockedChannels",
                newName: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BrandCategories_Brands_BrandsId",
                table: "BrandCategories",
                column: "BrandsId",
                principalTable: "Brands",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_BrandCategories_Categories_CategoriesId",
                table: "BrandCategories",
                column: "CategoriesId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
