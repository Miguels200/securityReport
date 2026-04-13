using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTipoReporte : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposReporte",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposReporte", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "TiposReporte",
                columns: new[] { "Id", "Nombre", "Descripcion" },
                values: new object[,]
                {
                    { new Guid("d1d1d1d1-d1d1-d1d1-d1d1-d1d1d1d1d1d1"), "Condición Insegura", "Situación o factor físico del entorno que puede causar un accidente" },
                    { new Guid("d2d2d2d2-d2d2-d2d2-d2d2-d2d2d2d2d2d2"), "Acto Inseguro", "Comportamiento o acción de una persona que aumenta el riesgo de accidente" },
                    { new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3"), "Incidente / Accidente", "Evento que resultó o pudo resultar en lesión, daño o pérdida" }
                });

            migrationBuilder.AddColumn<Guid>(
                name: "TipoReporteId",
                table: "Reportes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_TipoReporteId",
                table: "Reportes",
                column: "TipoReporteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Reportes_TiposReporte_TipoReporteId",
                table: "Reportes",
                column: "TipoReporteId",
                principalTable: "TiposReporte",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reportes_TiposReporte_TipoReporteId",
                table: "Reportes");

            migrationBuilder.DropIndex(
                name: "IX_Reportes_TipoReporteId",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "TipoReporteId",
                table: "Reportes");

            migrationBuilder.DeleteData(
                table: "TiposReporte",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("d1d1d1d1-d1d1-d1d1-d1d1-d1d1d1d1d1d1"),
                    new Guid("d2d2d2d2-d2d2-d2d2-d2d2-d2d2d2d2d2d2"),
                    new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3")
                });

            migrationBuilder.DropTable(
                name: "TiposReporte");
        }
    }
}
