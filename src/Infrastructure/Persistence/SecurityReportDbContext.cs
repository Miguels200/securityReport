using System;
using Microsoft.EntityFrameworkCore;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Infrastructure.Persistence
{
    public class SecurityReportDbContext : DbContext
    {
        public SecurityReportDbContext(DbContextOptions<SecurityReportDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Rol> Roles { get; set; } = null!;
        public DbSet<Permiso> Permisos { get; set; } = null!;
        public DbSet<RolPermiso> RolesPermiso { get; set; } = null!;
        public DbSet<Area> Areas { get; set; } = null!;
        public DbSet<Reporte> Reportes { get; set; } = null!;
        public DbSet<CondicionInsegura> Condiciones { get; set; } = null!;
        public DbSet<ActoInseguro> Actos { get; set; } = null!;
        public DbSet<Evidencia> Evidencias { get; set; } = null!;
        public DbSet<EstadoReporte> EstadosReporte { get; set; } = null!;
        public DbSet<TipoReporte> TiposReporte { get; set; } = null!;
        public DbSet<AnalisisIA> Analisis { get; set; } = null!;
        public DbSet<InformeIA> Informes { get; set; } = null!;
        public DbSet<RiesgoRepetitivo> RiesgosRepetitivos { get; set; } = null!;
        public DbSet<NormativaSGSST> Normativas { get; set; } = null!;
        public DbSet<LogAuditoria> LogsAuditoria { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Use deterministic GUIDs for seeded data so we can reference them later
            var rolAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
            var rolRespId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var rolColabId = Guid.Parse("33333333-3333-3333-3333-333333333333");
            var rolOperarioId = Guid.Parse("44444444-4444-4444-4444-444444444444");
            var rolSupervisorId = Guid.Parse("55555555-5555-5555-5555-555555555555");

            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = rolAdminId, Nombre = "Administrador" },
                new Rol { Id = rolRespId, Nombre = "ResponsableSST" },
                new Rol { Id = rolColabId, Nombre = "Colaborador" },
                new Rol { Id = rolOperarioId, Nombre = "Operario" },
                new Rol { Id = rolSupervisorId, Nombre = "Supervisor" }
            );

            var permisoCrearReporteId = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000001");
            var permisoEditarReporteId = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000002");
            var permisoEliminarReporteId = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000003");
            var permisoVerDashboardId = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000004");
            var permisoGestionarUsuariosId = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000005");
            var permisoVerReportesId = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000006");
            var permisoGenerarPlanIaId = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000007");
            var permisoAccederAdminId = Guid.Parse("a1a1a1a1-0000-0000-0000-000000000008");

            modelBuilder.Entity<Permiso>().HasData(
                new Permiso { Id = permisoCrearReporteId, Codigo = "crear_reporte", Nombre = "Crear Reportes" },
                new Permiso { Id = permisoEditarReporteId, Codigo = "editar_reporte", Nombre = "Editar Reportes" },
                new Permiso { Id = permisoEliminarReporteId, Codigo = "eliminar_reporte", Nombre = "Eliminar Reportes" },
                new Permiso { Id = permisoVerDashboardId, Codigo = "ver_dashboard", Nombre = "Ver Dashboard" },
                new Permiso { Id = permisoGestionarUsuariosId, Codigo = "gestionar_usuarios", Nombre = "Gestionar Usuarios" },
                new Permiso { Id = permisoVerReportesId, Codigo = "ver_reportes", Nombre = "Ver Reportes otros usuarios" },
                new Permiso { Id = permisoGenerarPlanIaId, Codigo = "generar_plan_ia", Nombre = "Generar Plan de Acción con IA" },
                new Permiso { Id = permisoAccederAdminId, Codigo = "acceder_administracion", Nombre = "Acceder a Administración" }
            );

            var estadoAbierto = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var estadoEnProgreso = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var estadoCerrado = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

            modelBuilder.Entity<EstadoReporte>().HasData(
                new EstadoReporte { Id = estadoAbierto, Nombre = "Abierto" },
                new EstadoReporte { Id = estadoEnProgreso, Nombre = "EnProgreso" },
                new EstadoReporte { Id = estadoCerrado, Nombre = "Cerrado" }
            );

            var tipoCondicionId = Guid.Parse("d1d1d1d1-d1d1-d1d1-d1d1-d1d1d1d1d1d1");
            var tipoActoId = Guid.Parse("d2d2d2d2-d2d2-d2d2-d2d2-d2d2d2d2d2d2");
            var tipoIncidenteId = Guid.Parse("d3d3d3d3-d3d3-d3d3-d3d3-d3d3d3d3d3d3");

            modelBuilder.Entity<TipoReporte>().HasData(
                new TipoReporte { Id = tipoCondicionId, Nombre = "Condición Insegura", Descripcion = "Situación o factor físico del entorno que puede causar un accidente" },
                new TipoReporte { Id = tipoActoId, Nombre = "Acto Inseguro", Descripcion = "Comportamiento o acción de una persona que aumenta el riesgo de accidente" },
                new TipoReporte { Id = tipoIncidenteId, Nombre = "Incidente / Accidente", Descripcion = "Evento que resultó o pudo resultar en lesión, daño o pérdida" }
            );

            // Configure relationships and indexes
            modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Reporte>().HasIndex(r => r.FechaReporte);

            modelBuilder.Entity<Permiso>().HasIndex(p => p.Codigo).IsUnique();

            modelBuilder.Entity<RolPermiso>()
                .HasKey(rp => new { rp.RolId, rp.PermisoId });

            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Rol)
                .WithMany()
                .HasForeignKey(rp => rp.RolId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolPermiso>()
                .HasOne(rp => rp.Permiso)
                .WithMany(p => p.RolesPermiso)
                .HasForeignKey(rp => rp.PermisoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<RolPermiso>().HasData(
                // Operario
                new RolPermiso { RolId = rolOperarioId, PermisoId = permisoCrearReporteId },

                // Supervisor
                new RolPermiso { RolId = rolSupervisorId, PermisoId = permisoCrearReporteId },
                new RolPermiso { RolId = rolSupervisorId, PermisoId = permisoEditarReporteId },
                new RolPermiso { RolId = rolSupervisorId, PermisoId = permisoVerDashboardId },
                new RolPermiso { RolId = rolSupervisorId, PermisoId = permisoVerReportesId },

                // Responsable SST
                new RolPermiso { RolId = rolRespId, PermisoId = permisoCrearReporteId },
                new RolPermiso { RolId = rolRespId, PermisoId = permisoEditarReporteId },
                new RolPermiso { RolId = rolRespId, PermisoId = permisoEliminarReporteId },
                new RolPermiso { RolId = rolRespId, PermisoId = permisoVerDashboardId },
                new RolPermiso { RolId = rolRespId, PermisoId = permisoVerReportesId },
                new RolPermiso { RolId = rolRespId, PermisoId = permisoGenerarPlanIaId },

                // Administrador
                new RolPermiso { RolId = rolAdminId, PermisoId = permisoCrearReporteId },
                new RolPermiso { RolId = rolAdminId, PermisoId = permisoEditarReporteId },
                new RolPermiso { RolId = rolAdminId, PermisoId = permisoEliminarReporteId },
                new RolPermiso { RolId = rolAdminId, PermisoId = permisoVerDashboardId },
                new RolPermiso { RolId = rolAdminId, PermisoId = permisoGestionarUsuariosId },
                new RolPermiso { RolId = rolAdminId, PermisoId = permisoVerReportesId },
                new RolPermiso { RolId = rolAdminId, PermisoId = permisoGenerarPlanIaId },
                new RolPermiso { RolId = rolAdminId, PermisoId = permisoAccederAdminId }
            );

            // Additional configuration as needed
        }
    }
}