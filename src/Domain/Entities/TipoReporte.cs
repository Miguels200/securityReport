using System;

namespace SecurityReport.Domain.Entities
{
    public class TipoReporte
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
    }
}
