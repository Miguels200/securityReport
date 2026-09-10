using System;

namespace SecurityReport.Domain.Entities
{
    public class AnalisisIA
    {
        public Guid Id { get; set; }
        public Guid ReporteId { get; set; }
        public Reporte? Reporte { get; set; }
        public string Tipo { get; set; } = string.Empty; // clasificacion_riesgo, analisis_general, similitud, repetitivo, predictivo, estadistico
        public string ResultadoJson { get; set; } = string.Empty; // copia integra del resultado, para auditoria

        // Campos estructurados de la clasificacion de riesgo (ver Application.DTOs.RiskClassificationResult)
        public TipoRiesgo? TipoRiesgo { get; set; }
        public NivelRiesgo? NivelRiesgo { get; set; }
        public PrioridadRiesgo? Prioridad { get; set; }
        public string? Justificacion { get; set; }
        public string? RecomendacionesJson { get; set; } // lista de strings serializada

        // Origen real del resultado: unicamente AZURE_OPENAI cuenta como prediccion de IA para metricas de tesis
        public OrigenAnalisis? Origen { get; set; }

        // Mensaje corto de diagnostico (sin secretos ni stack trace) cuando Status = Failed
        public string? ErrorMensaje { get; set; }

        public DateTime CreatedAt { get; set; }

        // Processing status for background worker
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
        public int AttemptCount { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}