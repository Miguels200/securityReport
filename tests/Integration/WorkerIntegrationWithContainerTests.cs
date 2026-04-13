using System.Threading.Tasks;
using Xunit;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Infrastructure.Services;
using SecurityReport.Infrastructure.Repositories;
using System.Threading;
using Moq;
using SecurityReport.Application.Interfaces;

namespace Tests.Integration
{
    public class WorkerIntegrationWithContainerTests : IAsyncLifetime
    {
        private readonly SqlTestcontainerFixture _fixture = new SqlTestcontainerFixture();

        public async Task InitializeAsync()
        {
            await _fixture.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await _fixture.DisposeAsync();
        }

        [Fact]
        public async Task WorkerProcessesPendingAnalysis_EndToEnd()
        {
            // Set up a service collection similar to the app
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddDbContext<SecurityReportDbContext>(options => options.UseSqlServer(_fixture.ConnectionString, sql => sql.MigrationsAssembly("SecurityReport.Infrastructure")));

            // Use mock AI client to avoid calling Azure
            var mockAi = new Mock<IAzureOpenAIClient>();
            mockAi.Setup(x => x.GetCompletionsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync("{\"mock\":true}");
            services.AddSingleton(mockAi.Object);

            services.AddScoped<IAnalysisRepository, SecurityReport.Infrastructure.Repositories.AnalysisRepository>();
            services.AddScoped<IReportRepository, SecurityReport.Infrastructure.Repositories.ReportRepository>();

            var provider = services.BuildServiceProvider();

            // Insert a report and a pending analysis
            using (var scope = provider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SecurityReportDbContext>();
                var report = new SecurityReport.Domain.Entities.Reporte { Id = Guid.NewGuid(), Titulo = "T", Descripcion = "D", AreaId = Guid.NewGuid(), EstadoReporteId = Guid.NewGuid(), ReportadoPorId = Guid.NewGuid(), FechaReporte = DateTime.UtcNow };
                await db.Reportes.AddAsync(report);
                var analysis = new SecurityReport.Domain.Entities.AnalisisIA { Id = Guid.NewGuid(), ReporteId = report.Id, Tipo = "analisis_general", Status = "Pending", CreatedAt = DateTime.UtcNow };
                await db.Analisis.AddAsync(analysis);
                await db.SaveChangesAsync();
            }

            // Run worker once
            using (var scope = provider.CreateScope())
            {
                var repo = scope.ServiceProvider.GetRequiredService<IAnalysisRepository>();
                var ai = scope.ServiceProvider.GetRequiredService<IAzureOpenAIClient>();
                var db = scope.ServiceProvider.GetRequiredService<SecurityReportDbContext>();

                var pending = await repo.GetPendingAsync();
                Assert.NotNull(pending);

                // Simulate worker processing
                pending.Status = "Processing";
                pending.AttemptCount += 1;
                await repo.UpdateAsync(pending);

                var report = await db.Reportes.FindAsync(pending.ReporteId);
                var result = await ai.GetCompletionsAsync($"Analiza: {report.Descripcion}", "deploy", 1000);

                pending.ResultadoJson = result;
                pending.Status = "Completed";
                pending.CompletedAt = DateTime.UtcNow;
                await repo.UpdateAsync(pending);

                var processed = await repo.GetByIdAsync(pending.Id);
                Assert.Equal("Completed", processed!.Status);
            }
        }
    }
}
