using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;
using SecurityReport.Application.DTOs;
using SecurityReport.Application.Interfaces;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Background;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Infrastructure.Repositories;

namespace Tests.Integration
{
    public class AnalysisMessageHandlerTests
    {
        private static (ServiceProvider provider, Guid reporteId, Guid analysisId) SeedProvider(
            Mock<IRiskClassificationService> classificationMock)
        {
            var dbName = Guid.NewGuid().ToString();
            // InMemoryDatabaseRoot explicito: sin el, cada scope/ServiceProvider puede terminar
            // usando un almacen aislado aunque el nombre de la BD sea el mismo.
            var root = new InMemoryDatabaseRoot();
            var services = new ServiceCollection();
            services.AddDbContext<SecurityReportDbContext>(o => o.UseInMemoryDatabase(dbName, root));
            services.AddScoped<IAnalysisRepository, AnalysisRepository>();
            services.AddSingleton(classificationMock.Object);

            var provider = services.BuildServiceProvider();

            var reporteId = Guid.NewGuid();
            var analysisId = Guid.NewGuid();

            using (var scope = provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SecurityReportDbContext>();
                var areaId = Guid.NewGuid();
                db.Areas.Add(new Area { Id = areaId, Nombre = "Area de prueba" });
                db.Reportes.Add(new Reporte
                {
                    Id = reporteId,
                    Titulo = "Reporte de prueba",
                    Descripcion = "Descripcion de prueba",
                    AreaId = areaId,
                    EstadoReporteId = Guid.NewGuid(),
                    ReportadoPorId = Guid.NewGuid(),
                    FechaReporte = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
                db.Analisis.Add(new AnalisisIA
                {
                    Id = analysisId,
                    ReporteId = reporteId,
                    Tipo = "clasificacion_riesgo",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                });
                db.SaveChanges();
            }

            return (provider, reporteId, analysisId);
        }

        private static AnalysisMessageHandler NewHandler(ServiceProvider provider) =>
            new(provider, new ConfigurationBuilder().Build(), NullLogger<AnalysisMessageHandler>.Instance);

        [Fact]
        public async Task HandleAsync_SuccessfulClassification_MarksCompleted()
        {
            var classificationMock = new Mock<IRiskClassificationService>();
            classificationMock.Setup(s => s.ClassifyAsync(It.IsAny<RiskClassificationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RiskClassificationResult
                {
                    TipoRiesgo = TipoRiesgo.FISICO,
                    NivelRiesgo = NivelRiesgo.MEDIO,
                    Prioridad = PrioridadRiesgo.MEDIA,
                    Justificacion = "ok",
                    Origen = OrigenAnalisis.AZURE_OPENAI
                });

            var (provider, _, analysisId) = SeedProvider(classificationMock);
            var handler = NewHandler(provider);

            await handler.HandleAsync(analysisId);

            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SecurityReportDbContext>();
            var analysis = await db.Analisis.FindAsync(analysisId);

            Assert.Equal("Completed", analysis!.Status);
            Assert.NotNull(analysis.StartedAt);
            Assert.NotNull(analysis.CompletedAt);
            Assert.Equal(OrigenAnalisis.AZURE_OPENAI, analysis.Origen);
        }

        [Fact]
        public async Task HandleAsync_OrigenError_MarksFailed()
        {
            var classificationMock = new Mock<IRiskClassificationService>();
            classificationMock.Setup(s => s.ClassifyAsync(It.IsAny<RiskClassificationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RiskClassificationResult
                {
                    Origen = OrigenAnalisis.ERROR,
                    ErrorMensaje = "fallo simulado"
                });

            var (provider, _, analysisId) = SeedProvider(classificationMock);
            var handler = NewHandler(provider);

            await handler.HandleAsync(analysisId);

            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SecurityReportDbContext>();
            var analysis = await db.Analisis.FindAsync(analysisId);

            Assert.Equal("Failed", analysis!.Status);
            Assert.NotNull(analysis.CompletedAt);
        }

        [Fact]
        public async Task HandleAsync_UnhandledException_MarksFailed_NeverStaysProcessing()
        {
            var classificationMock = new Mock<IRiskClassificationService>();
            classificationMock.Setup(s => s.ClassifyAsync(It.IsAny<RiskClassificationRequest>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("boom"));

            var (provider, _, analysisId) = SeedProvider(classificationMock);
            var handler = NewHandler(provider);

            await handler.HandleAsync(analysisId);

            using var scope = provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SecurityReportDbContext>();
            var analysis = await db.Analisis.FindAsync(analysisId);

            Assert.Equal("Failed", analysis!.Status);
            Assert.NotNull(analysis.CompletedAt);
        }

        [Fact]
        public async Task HandleAsync_CalledTwice_OnlyClassifiesOnce()
        {
            var classificationMock = new Mock<IRiskClassificationService>();
            classificationMock.Setup(s => s.ClassifyAsync(It.IsAny<RiskClassificationRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new RiskClassificationResult { Origen = OrigenAnalisis.AZURE_OPENAI, NivelRiesgo = NivelRiesgo.BAJO, TipoRiesgo = TipoRiesgo.FISICO, Prioridad = PrioridadRiesgo.BAJA });

            var (provider, _, analysisId) = SeedProvider(classificationMock);
            var handler = NewHandler(provider);

            await handler.HandleAsync(analysisId);
            await handler.HandleAsync(analysisId); // segunda "entrega" del mismo mensaje

            classificationMock.Verify(
                s => s.ClassifyAsync(It.IsAny<RiskClassificationRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
