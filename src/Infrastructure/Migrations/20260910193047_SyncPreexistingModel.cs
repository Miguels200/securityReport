using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncPreexistingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observaciones",
                table: "Reportes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PersonasAfectadas",
                table: "Reportes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PlanAccionJson",
                table: "Reportes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TieneTestigos",
                table: "Reportes",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TipoReporteId",
                table: "Reportes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "RolesPermiso",
                columns: table => new
                {
                    RolId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermisoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesPermiso", x => new { x.RolId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_RolesPermiso_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolesPermiso_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Permisos",
                columns: new[] { "Id", "Codigo", "Nombre" },
                values: new object[,]
                {
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000001"), "crear_reporte", "Crear Reportes" },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000002"), "editar_reporte", "Editar Reportes" },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000003"), "eliminar_reporte", "Eliminar Reportes" },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000004"), "ver_dashboard", "Ver Dashboard" },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000005"), "gestionar_usuarios", "Gestionar Usuarios" },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000006"), "ver_reportes", "Ver Reportes otros usuarios" },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000007"), "generar_plan_ia", "Generar Plan de Acción con IA" },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000008"), "acceder_administracion", "Acceder a Administración" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Nombre" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), "Operario" },
                    { new Guid("55555555-5555-5555-5555-555555555555"), "Supervisor" }
                });

            migrationBuilder.InsertData(
                table: "TiposReporte",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { new Guid("d1d1d1d1-d1d1-d1d1-d1d1-d1d1d1d1d1d1"), "Situación o factor físico del entorno que puede causar un accidente", "Condición Insegura" },
                    { new Guid("d2d2d2d2-d2d2-d2d2-d2d2-d2d2d2d2d2d2"), "Comportamiento o acción de una persona que aumenta el riesgo de accidente", "Acto Inseguro" },
                    { new Guid("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3"), "Evento que resultó o pudo resultar en lesión, daño o pérdida", "Incidente / Accidente" }
                });

            migrationBuilder.InsertData(
                table: "RolesPermiso",
                columns: new[] { "PermisoId", "RolId" },
                values: new object[,]
                {
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000005"), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000006"), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000007"), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000008"), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000001"), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000002"), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000003"), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000004"), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000006"), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000007"), new Guid("22222222-2222-2222-2222-222222222222") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000001"), new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000002"), new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000004"), new Guid("55555555-5555-5555-5555-555555555555") },
                    { new Guid("a1a1a1a1-0000-0000-0000-000000000006"), new Guid("55555555-5555-5555-5555-555555555555") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reportes_TipoReporteId",
                table: "Reportes",
                column: "TipoReporteId");

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_Codigo",
                table: "Permisos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermiso_PermisoId",
                table: "RolesPermiso",
                column: "PermisoId");

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

            migrationBuilder.DropTable(
                name: "RolesPermiso");

            migrationBuilder.DropTable(
                name: "TiposReporte");

            migrationBuilder.DropTable(
                name: "Permisos");

            migrationBuilder.DropIndex(
                name: "IX_Reportes_TipoReporteId",
                table: "Reportes");

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("44444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("55555555-5555-5555-5555-555555555555"));

            migrationBuilder.DropColumn(
                name: "Observaciones",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "PersonasAfectadas",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "PlanAccionJson",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "TieneTestigos",
                table: "Reportes");

            migrationBuilder.DropColumn(
                name: "TipoReporteId",
                table: "Reportes");
        }
    }
}
