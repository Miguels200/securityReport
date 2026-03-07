using System;

namespace SecurityReport.Domain.Entities
{
    public class ActoInseguro
    {
        public Guid Id { get; set; }
        public string Descripcion { get; set; } = string.Empty;
        public Guid ReporteId { get; set; }
        public Reporte? Reporte { get; set; }
        public DateTime FechaIdentificacion { get; set; }
    }
}