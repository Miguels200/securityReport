using System;
using System.Threading.Tasks;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Interfaces
{
    public interface IReportRepository
    {
        Task AddAsync(Reporte reporte);
        Task<Reporte?> GetByIdAsync(Guid id);
        Task UpdateAsync(Reporte reporte);
        Task DeleteAsync(Reporte reporte);
    }
}