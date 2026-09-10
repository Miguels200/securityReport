using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SecurityReport.Api.Controllers;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Persistence;

namespace Tests.Integration
{
    public class AnalisisControllerAuthorizationTests
    {
        private static SecurityReportDbContext NewInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<SecurityReportDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new SecurityReportDbContext(options);
        }

        private static AnalisisController NewController(SecurityReportDbContext db, ClaimsPrincipal user)
        {
            var controller = new AnalisisController(null!, db);
            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
            return controller;
        }

        private static ClaimsPrincipal BuildUser(string email, string rol)
        {
            var identity = new ClaimsIdentity(new[]
            {
                new Claim("sub", email),
                new Claim(ClaimTypes.Role, rol)
            }, "Test");
            return new ClaimsPrincipal(identity);
        }

        private static async Task<(Guid reporteId, Guid analysisId, Guid autorId)> SeedAsync(SecurityReportDbContext db)
        {
            var autorId = Guid.NewGuid();
            var reporteId = Guid.NewGuid();
            var analysisId = Guid.NewGuid();

            db.Usuarios.Add(new Usuario { Id = autorId, Nombre = "Autor", Email = "autor@empresa.com", RolId = Guid.NewGuid() });
            db.Reportes.Add(new Reporte
            {
                Id = reporteId,
                Titulo = "T",
                Descripcion = "D",
                AreaId = Guid.NewGuid(),
                EstadoReporteId = Guid.NewGuid(),
                ReportadoPorId = autorId,
                FechaReporte = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            db.Analisis.Add(new AnalisisIA
            {
                Id = analysisId,
                ReporteId = reporteId,
                Tipo = "clasificacion_riesgo",
                Status = "Completed",
                CreatedAt = DateTime.UtcNow,
                NivelRiesgo = NivelRiesgo.MEDIO,
                TipoRiesgo = TipoRiesgo.FISICO,
                Prioridad = PrioridadRiesgo.MEDIA
            });
            await db.SaveChangesAsync();

            return (reporteId, analysisId, autorId);
        }

        [Fact]
        public async Task Get_AsOwner_ReturnsOk()
        {
            using var db = NewInMemoryContext(Guid.NewGuid().ToString());
            var (_, analysisId, _) = await SeedAsync(db);

            var controller = NewController(db, BuildUser("autor@empresa.com", "Operario"));
            var result = await controller.Get(analysisId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Get_AsResponsableSST_ReturnsOk_EvenIfNotOwner()
        {
            using var db = NewInMemoryContext(Guid.NewGuid().ToString());
            var (_, analysisId, _) = await SeedAsync(db);

            var controller = NewController(db, BuildUser("otro@empresa.com", "ResponsableSST"));
            var result = await controller.Get(analysisId);

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task Get_AsUnrelatedUser_ReturnsForbid()
        {
            using var db = NewInMemoryContext(Guid.NewGuid().ToString());
            var (_, analysisId, _) = await SeedAsync(db);

            var controller = NewController(db, BuildUser("otro@empresa.com", "Operario"));
            var result = await controller.Get(analysisId);

            Assert.IsType<ForbidResult>(result);
        }

        [Fact]
        public async Task Get_UnknownId_ReturnsNotFound()
        {
            using var db = NewInMemoryContext(Guid.NewGuid().ToString());
            await SeedAsync(db);

            var controller = NewController(db, BuildUser("autor@empresa.com", "Operario"));
            var result = await controller.Get(Guid.NewGuid());

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
