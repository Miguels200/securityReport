using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SecurityReport.Application.DTOs;
using SecurityReport.Application.Interfaces;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Infrastructure.Background
{
    public class AnalysisMessageHandler
    {
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _config;
        private readonly ILogger<AnalysisMessageHandler> _logger;

        public AnalysisMessageHandler(IServiceProvider provider, IConfiguration config, ILogger<AnalysisMessageHandler> logger)
        {
            _provider = provider;
            _config = config;
            _logger = logger;
        }

        public async Task HandleAsync(Guid analysisId)
        {
            using var scope = _provider.CreateScope();
            var analysisRepo = scope.ServiceProvider.GetRequiredService<IAnalysisRepository>();
            var classificationService = scope.ServiceProvider.GetRequiredService<IRiskClassificationService>();
            var db = scope.ServiceProvider.GetRequiredService<SecurityReport.Infrastructure.Persistence.SecurityReportDbContext>();

            // Reclamo atomico Pending -> Processing. Si otro worker ya lo tomo, no se llama a Azure OpenAI.
            var claimed = await analysisRepo.TryMarkProcessingAsync(analysisId);
            if (!claimed)
            {
                _logger.LogInformation("Analysis {Id} ya fue reclamado por otro worker o no esta Pending; se omite.", analysisId);
                return;
            }

            var analysis = await analysisRepo.GetByIdAsync(analysisId);
            if (analysis == null)
            {
                _logger.LogWarning("Analysis {Id} no encontrado tras reclamarlo", analysisId);
                return;
            }

            var report = await db.Reportes
                .Include(r => r.Area)
                .Include(r => r.TipoReporte)
                .FirstOrDefaultAsync(r => r.Id == analysis.ReporteId);

            if (report == null)
            {
                analysis.Status = "Failed";
                analysis.ErrorMensaje = "El reporte asociado ya no existe.";
                analysis.CompletedAt = DateTime.UtcNow;
                await analysisRepo.UpdateAsync(analysis);
                return;
            }

            try
            {
                var request = new RiskClassificationRequest(
                    report.Id,
                    report.Titulo,
                    report.Descripcion,
                    report.Area?.Nombre ?? string.Empty,
                    report.TipoReporte?.Nombre ?? string.Empty,
                    string.Empty,
                    string.Empty,
                    report.PersonasAfectadas,
                    report.TieneTestigos);

                var result = await classificationService.ClassifyAsync(request);

                analysis.TipoRiesgo = result.TipoRiesgo;
                analysis.NivelRiesgo = result.NivelRiesgo;
                analysis.Prioridad = result.Prioridad;
                analysis.Justificacion = result.Justificacion;
                analysis.RecomendacionesJson = JsonSerializer.Serialize(result.Recomendaciones);
                analysis.Origen = result.Origen;
                analysis.ErrorMensaje = result.ErrorMensaje;
                analysis.ResultadoJson = JsonSerializer.Serialize(result);
                // Solo ERROR (fallo definitivo de Azure OpenAI) se marca Failed; NULL_CLIENT y HEURISTIC
                // producen un resultado utilizable (con Origen claramente distinguible) y se marcan Completed.
                analysis.Status = result.Origen == OrigenAnalisis.ERROR ? "Failed" : "Completed";
                analysis.CompletedAt = DateTime.UtcNow;
                await analysisRepo.UpdateAsync(analysis);

                _logger.LogInformation("Analysis {Id} procesado con Origen={Origen}, Status={Status}", analysisId, result.Origen, analysis.Status);
            }
            catch (Exception ex)
            {
                // Defensa adicional: cualquier fallo no controlado por el servicio de clasificacion
                // tambien debe cerrar el analisis como Failed, nunca dejarlo colgado en Processing.
                _logger.LogError(ex, "Error inesperado procesando analysis {Id}", analysisId);
                analysis.Status = "Failed";
                analysis.ErrorMensaje = "Error inesperado durante el analisis.";
                analysis.CompletedAt = DateTime.UtcNow;
                await analysisRepo.UpdateAsync(analysis);
            }
        }
    }
}

