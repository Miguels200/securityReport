using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Infrastructure.Repositories;

namespace Tests.Integration
{
    public class AnalysisRepositoryConcurrencyTests
    {
        private static SecurityReportDbContext NewInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<SecurityReportDbContext>()
                .UseInMemoryDatabase(dbName)
                .Options;
            return new SecurityReportDbContext(options);
        }

        [Fact]
        public async Task TryMarkProcessingAsync_SecondCaller_CannotClaimAlreadyProcessingAnalysis()
        {
            var dbName = Guid.NewGuid().ToString();
            var analysisId = Guid.NewGuid();

            await using (var seedDb = NewInMemoryContext(dbName))
            {
                seedDb.Analisis.Add(new AnalisisIA
                {
                    Id = analysisId,
                    ReporteId = Guid.NewGuid(),
                    Tipo = "clasificacion_riesgo",
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                });
                await seedDb.SaveChangesAsync();
            }

            // Simula dos "workers" con su propio DbContext apuntando a la misma base InMemory.
            await using var dbWorker1 = NewInMemoryContext(dbName);
            await using var dbWorker2 = NewInMemoryContext(dbName);
            var repoWorker1 = new AnalysisRepository(dbWorker1);
            var repoWorker2 = new AnalysisRepository(dbWorker2);

            var claimedByWorker1 = await repoWorker1.TryMarkProcessingAsync(analysisId);
            var claimedByWorker2 = await repoWorker2.TryMarkProcessingAsync(analysisId);

            Assert.True(claimedByWorker1);
            Assert.False(claimedByWorker2);
        }

        [Fact]
        public async Task TryMarkProcessingAsync_ReturnsFalse_WhenAnalysisDoesNotExist()
        {
            await using var db = NewInMemoryContext(Guid.NewGuid().ToString());
            var repo = new AnalysisRepository(db);

            var claimed = await repo.TryMarkProcessingAsync(Guid.NewGuid());

            Assert.False(claimed);
        }
    }
}
