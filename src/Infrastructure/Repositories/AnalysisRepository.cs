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
    }
}