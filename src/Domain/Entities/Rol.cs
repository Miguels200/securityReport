using System;
using System.Collections.Generic;

namespace SecurityReport.Domain.Entities
{
    public class Rol
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public ICollection<Usuario>? Usuarios { get; set; }
    }
}