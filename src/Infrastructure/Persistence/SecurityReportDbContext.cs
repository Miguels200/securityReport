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
        public DbSet<Area> Areas { get; set; } = null!;
        public DbSet<Reporte> Reportes { get; set; } = null!;
        public DbSet<CondicionInsegura> Condiciones { get; set; } = null!;
        public DbSet<ActoInseguro> Actos { get; set; } = null!;
        public DbSet<Evidencia> Evidencias { get; set; } = null!;
        public DbSet<EstadoReporte> EstadosReporte { get; set; } = null!;
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

            modelBuilder.Entity<Rol>().HasData(
                new Rol { Id = rolAdminId, Nombre = "Administrador" },
                new Rol { Id = rolRespId, Nombre = "ResponsableSST" },
                new Rol { Id = rolColabId, Nombre = "Colaborador" }
            );

            var estadoAbierto = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
            var estadoEnProgreso = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
            var estadoCerrado = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

            modelBuilder.Entity<EstadoReporte>().HasData(
                new EstadoReporte { Id = estadoAbierto, Nombre = "Abierto" },
                new EstadoReporte { Id = estadoEnProgreso, Nombre = "EnProgreso" },
                new EstadoReporte { Id = estadoCerrado, Nombre = "Cerrado" }
            );

            // Configure relationships and indexes
            modelBuilder.Entity<Usuario>().HasIndex(u => u.Email).IsUnique();
            modelBuilder.Entity<Reporte>().HasIndex(r => r.FechaReporte);

            // Additional configuration as needed
        }
    }
}