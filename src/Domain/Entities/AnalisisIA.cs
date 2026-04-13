using System;

namespace SecurityReport.Domain.Entities
{
    public class AnalisisIA
    {
        public Guid Id { get; set; }
        public Guid ReporteId { get; set; }
        public Reporte? Reporte { get; set; }
        public string Tipo { get; set; } = string.Empty; // similitud, repetitivo, predictivo, estadistico
        public string ResultadoJson { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty; // AzureOpenAI
        public DateTime CreatedAt { get; set; }

        // Processing status for background worker
        public string Status { get; set; } = "Pending"; // Pending, Processing, Completed, Failed
        public int AttemptCount { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
    }
}