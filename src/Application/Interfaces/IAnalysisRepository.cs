using System.Threading.Tasks;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Interfaces
{
    public interface IAnalysisRepository
    {
        Task AddAsync(AnalisisIA analysis);
        Task<AnalisisIA?> GetPendingAsync();
        Task<AnalisisIA?> GetByIdAsync(System.Guid id);
        Task UpdateAsync(AnalisisIA analysis);
    }
}