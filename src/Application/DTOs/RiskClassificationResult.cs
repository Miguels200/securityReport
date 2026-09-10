using System;
using System.Collections.Generic;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.DTOs
{
    // Resultado tipado de la clasificacion de riesgo. Prioridad siempre se deriva en backend
    // (ver Application.Common.PrioridadMapper), nunca se solicita ni se acepta del modelo de IA.
    public class RiskClassificationResult
    {
        public TipoRiesgo? TipoRiesgo { get; set; }
        public NivelRiesgo? NivelRiesgo { get; set; }
        public PrioridadRiesgo? Prioridad { get; set; }
        public string Justificacion { get; set; } = string.Empty;
        public List<string> Recomendaciones { get; set; } = new();

        // Origen real del resultado. Solo AZURE_OPENAI cuenta como prediccion de IA para metricas.
        public OrigenAnalisis Origen { get; set; }

        // Mensaje corto de diagnostico (sin secretos) cuando Origen = ERROR.
        public string? ErrorMensaje { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
