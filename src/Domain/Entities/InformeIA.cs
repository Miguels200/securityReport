using System;

namespace SecurityReport.Domain.Entities
{
    public class InformeIA
    {
        public Guid Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string Periodo { get; set; } = string.Empty;
    }
}