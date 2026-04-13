using System;

namespace SecurityReport.Domain.Entities
{
    public class RolPermiso
    {
        public Guid RolId { get; set; }
        public Rol? Rol { get; set; }

        public Guid PermisoId { get; set; }
        public Permiso? Permiso { get; set; }
    }
}
