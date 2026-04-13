using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly SecurityReportDbContext _db;

        public ReportRepository(SecurityReportDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Reporte reporte)
        {
            await _db.Reportes.AddAsync(reporte);
            await _db.SaveChangesAsync();
        }

        public async Task<Reporte?> GetByIdAsync(Guid id)
        {
            return await _db.Reportes.Include(r => r.Analisis).FirstOrDefaultAsync(r => r.Id == id);
        }

        public async Task UpdateAsync(Reporte reporte)
        {
            _db.Reportes.Update(reporte);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Reporte reporte)
        {
            _db.Reportes.Remove(reporte);
            await _db.SaveChangesAsync();
        }
    }
}