using System;

namespace SecurityReport.Domain.Entities
{
    public class RiesgoRepetitivo
    {
        public Guid Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public int Ocurrencias { get; set; }
        public string NivelRiesgo { get; set; } = string.Empty; // Bajo, Medio, Alto
        public DateTime FirstDetected { get; set; }
        public DateTime LastDetected { get; set; }
    }
}