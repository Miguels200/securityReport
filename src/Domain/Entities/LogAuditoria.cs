using System;

namespace SecurityReport.Domain.Entities
{
    public class LogAuditoria
    {
        public Guid Id { get; set; }
        public string Entidad { get; set; } = string.Empty;
        public Guid EntidadId { get; set; }
        public string Accion { get; set; } = string.Empty; // Create, Update, Delete
        public string Usuario { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Detalle { get; set; } = string.Empty;
    }
}