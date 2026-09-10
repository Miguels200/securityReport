using System.Threading;
using System.Threading.Tasks;
using SecurityReport.Application.DTOs;

namespace SecurityReport.Application.Interfaces
{
    // Responsabilidad exclusiva: clasificar el riesgo de un reporte mediante IA (Azure OpenAI ya existente).
    // No reemplaza IAIAnalysisService (que conserva la generacion del Plan de Accion).
    public interface IRiskClassificationService
    {
        Task<RiskClassificationResult> ClassifyAsync(RiskClassificationRequest request, CancellationToken cancellationToken = default);
    }
}
