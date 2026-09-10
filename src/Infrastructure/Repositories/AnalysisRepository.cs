using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Infrastructure.Repositories
{
    public class AnalysisRepository : IAnalysisRepository
    {
        private readonly SecurityReportDbContext _db;

        public AnalysisRepository(SecurityReportDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(AnalisisIA analysis)
        {
            await _db.Analisis.AddAsync(analysis);
            await _db.SaveChangesAsync();
        }

        public async Task<AnalisisIA?> GetPendingAsync()
        {
            return await _db.Analisis.FirstOrDefaultAsync(a => a.Status == "Pending");
        }

        public async Task<AnalisisIA?> GetByIdAsync(System.Guid id)
        {
            return await _db.Analisis.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task UpdateAsync(AnalisisIA analysis)
        {
            _db.Analisis.Update(analysis);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> TryMarkProcessingAsync(System.Guid id)
        {
            var now = System.DateTime.UtcNow;

            if (_db.Database.IsRelational())
            {
                // UPDATE ... WHERE Status = 'Pending' atomico: rowsAffected == 1 solo para quien lo reclama primero.
                var rows = await _db.Analisis
                    .Where(a => a.Id == id && a.Status == "Pending")
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(a => a.Status, "Processing")
                        .SetProperty(a => a.StartedAt, now)
                        .SetProperty(a => a.AttemptCount, a => a.AttemptCount + 1));
                return rows == 1;
            }

            // Proveedor InMemory (dev/tests): ExecuteUpdateAsync no esta soportado.
            // No es atomico entre procesos, pero es suficiente para un unico proceso host de pruebas.
            var analysis = await _db.Analisis.FirstOrDefaultAsync(a => a.Id == id && a.Status == "Pending");
            if (analysis == null) return false;

            analysis.Status = "Processing";
            analysis.StartedAt = now;
            analysis.AttemptCount += 1;
            await _db.SaveChangesAsync();
            return true;
        }
    }
}