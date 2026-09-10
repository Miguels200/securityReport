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

        // Actualizacion atomica Pending -> Processing. Devuelve true solo si ESTE llamador
        // fue quien logro reclamar el registro (rowsAffected == 1); false si ya no estaba Pending
        // (otro worker lo tomo primero o no existe). El llamador NUNCA debe invocar Azure OpenAI
        // si el resultado es false. Es el mecanismo que evita el doble procesamiento entre workers.
        Task<bool> TryMarkProcessingAsync(System.Guid id);
    }
}