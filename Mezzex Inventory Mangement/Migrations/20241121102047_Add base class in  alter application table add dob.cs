﻿using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mezzex_Inventory_Mangement.Migrations
{
    /// <inheritdoc />
    public partial class Addbaseclassinalterapplicationtableadddob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateOfBirth",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateOfBirth",
                table: "AspNetUsers");
        }
    }
}
