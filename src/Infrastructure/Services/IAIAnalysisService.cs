using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Infrastructure.Services
{
    public interface IAIAnalysisService
    {
        Task<string> AnalyzeReportTextAsync(Guid reporteId, string text, string tipo);
        Task<PlanAccionIAResult> GeneratePlanAccionAsync(PlanAccionRequest request);
    }

    public class PlanAccionRequest
    {
        public Guid ReporteId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string TipoReporte { get; set; } = string.Empty;
        public string NivelRiesgo { get; set; } = string.Empty;
        public string Area { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;
        public string Condicion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public int PersonasAfectadas { get; set; } = 1;
        public bool TieneTestigos { get; set; }
    }

    public class PlanAccionIAResult
    {
        public List<string> Acciones { get; set; } = new();
        public RecursosIA Recursos { get; set; } = new();
        public string Responsable { get; set; } = string.Empty;
        public TiempoEjecucionIA TiempoEjecucion { get; set; } = new();
        public List<string> NormativaAplicable { get; set; } = new();
        public string Disclaimer { get; set; } = string.Empty;
        // Conservado por compatibilidad con el frontend existente; se deriva de Origen == AZURE_OPENAI.
        public bool GeneradoConIA { get; set; }
        // Origen real del plan: unicamente AZURE_OPENAI proviene realmente del modelo de IA.
        public OrigenAnalisis Origen { get; set; }
        // Mensaje corto de diagnostico (sin secretos) cuando Origen = ERROR.
        public string? ErrorMensaje { get; set; }
    }

    public class RecursosIA
    {
        public string Economicos { get; set; } = string.Empty;
        public string Tiempo { get; set; } = string.Empty;
        public string Personal { get; set; } = string.Empty;
    }

    public class TiempoEjecucionIA
    {
        public string Tipo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int DiasEstimados { get; set; }
    }
}
