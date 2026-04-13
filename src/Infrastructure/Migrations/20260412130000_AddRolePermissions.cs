using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class AddRolePermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = '44444444-4444-4444-4444-444444444444')
BEGIN
    INSERT INTO [Roles] ([Id], [Nombre]) VALUES ('44444444-4444-4444-4444-444444444444', 'Operario')
END
");

            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Id] = '55555555-5555-5555-5555-555555555555')
BEGIN
    INSERT INTO [Roles] ([Id], [Nombre]) VALUES ('55555555-5555-5555-5555-555555555555', 'Supervisor')
END
");

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

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_Codigo",
                table: "Permisos",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolesPermiso_PermisoId",
                table: "RolesPermiso",
                column: "PermisoId");

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
                table: "RolesPermiso",
                columns: new[] { "RolId", "PermisoId" },
                values: new object[,]
                {
                    { new Guid("44444444-4444-4444-4444-444444444444"), new Guid("a1a1a1a1-0000-0000-0000-000000000001") },

                    { new Guid("55555555-5555-5555-5555-555555555555"), new Guid("a1a1a1a1-0000-0000-0000-000000000001") },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new Guid("a1a1a1a1-0000-0000-0000-000000000002") },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new Guid("a1a1a1a1-0000-0000-0000-000000000004") },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new Guid("a1a1a1a1-0000-0000-0000-000000000006") },

                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("a1a1a1a1-0000-0000-0000-000000000001") },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("a1a1a1a1-0000-0000-0000-000000000002") },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("a1a1a1a1-0000-0000-0000-000000000003") },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("a1a1a1a1-0000-0000-0000-000000000004") },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("a1a1a1a1-0000-0000-0000-000000000006") },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new Guid("a1a1a1a1-0000-0000-0000-000000000007") },

                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("a1a1a1a1-0000-0000-0000-000000000001") },
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("a1a1a1a1-0000-0000-0000-000000000002") },
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("a1a1a1a1-0000-0000-0000-000000000003") },
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("a1a1a1a1-0000-0000-0000-000000000004") },
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("a1a1a1a1-0000-0000-0000-000000000005") },
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("a1a1a1a1-0000-0000-0000-000000000006") },
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("a1a1a1a1-0000-0000-0000-000000000007") },
                    { new Guid("11111111-1111-1111-1111-111111111111"), new Guid("a1a1a1a1-0000-0000-0000-000000000008") }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RolesPermiso");
            migrationBuilder.DropTable(name: "Permisos");
        }
    }
}
