using System;
using System.Collections.Generic;

namespace SecurityReport.Domain.Entities
{
    public class Reporte
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string Observaciones { get; set; } = string.Empty;
        public int PersonasAfectadas { get; set; } = 1;
        public bool TieneTestigos { get; set; }
        public string? PlanAccionJson { get; set; }

        // Nivel de riesgo elegido manualmente por el usuario en el formulario.
        // Se conserva por separado del nivel determinado por IA (AnalisisIA.NivelRiesgo) para
        // permitir comparacion posterior; NO debe sobrescribirse ni usarse como sugerencia al modelo.
        public NivelRiesgo? NivelReportadoUsuario { get; set; }
        public Guid AreaId { get; set; }
        public Area? Area { get; set; }
        public Guid EstadoReporteId { get; set; }
        public EstadoReporte? EstadoReporte { get; set; }
        public Guid? TipoReporteId { get; set; }
        public TipoReporte? TipoReporte { get; set; }
        public Guid ReportadoPorId { get; set; }
        public Usuario? ReportadoPor { get; set; }
        public DateTime FechaReporte { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public ICollection<CondicionInsegura>? Condiciones { get; set; }
        public ICollection<ActoInseguro>? Actos { get; set; }
        public ICollection<Evidencia>? Evidencias { get; set; }
        public ICollection<AnalisisIA>? Analisis { get; set; }
    }
}