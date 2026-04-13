using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddReportePlanAccionMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PersonasAfectadas",
                table: "Reportes",
                type: "int",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<bool>(
                name: "TieneTestigos",
                table: "Reportes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PlanAccionJson",
                table: "Reportes",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PersonasAfectadas",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "TieneTestigos",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "PlanAccionJson",
                table: "Reportes");
        }
    }
}
