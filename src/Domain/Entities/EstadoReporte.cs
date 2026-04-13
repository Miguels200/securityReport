using System;

namespace SecurityReport.Domain.Entities
{
    public class EstadoReporte
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty; // Abierto, EnProgreso, Cerrado
    }
}