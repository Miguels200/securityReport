using System;
using System.Threading.Tasks;

namespace SecurityReport.Infrastructure.Services
{
    public interface IAIAnalysisService
    {
        Task<string> AnalyzeReportTextAsync(Guid reporteId, string text, string tipo);
    }
}