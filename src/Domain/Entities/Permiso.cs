using System;
using System.Collections.Generic;

namespace SecurityReport.Domain.Entities
{
    public class Permiso
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public ICollection<RolPermiso>? RolesPermiso { get; set; }
    }
}
