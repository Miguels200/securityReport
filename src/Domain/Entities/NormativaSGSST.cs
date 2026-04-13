using System;

namespace SecurityReport.Domain.Entities
{
    public class NormativaSGSST
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty; // ej: Ley 1562
        public string Titulo { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
    }
}