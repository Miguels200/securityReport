using System;
using System.Collections.Generic;

namespace SecurityReport.Domain.Entities
{
    public class Area
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public ICollection<Reporte>? Reportes { get; set; }
    }
}