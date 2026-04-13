using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/role-permissions")]
    [Authorize]
    public class RolePermissionsController : ControllerBase
    {
        private readonly SecurityReportDbContext _db;

        public RolePermissionsController(SecurityReportDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var roles = await _db.Roles.AsNoTracking().ToListAsync();
                var permisos = await _db.Permisos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
                var rolPermisos = await _db.RolesPermiso.AsNoTracking().ToListAsync();

                var roleMap = roles.ToDictionary(
                    r => NormalizarRol(r.Nombre),
                    r => rolPermisos
                        .Where(rp => rp.RolId == r.Id)
                        .Join(permisos, rp => rp.PermisoId, p => p.Id, (_, p) => p.Codigo)
                        .Distinct()
                        .OrderBy(x => x)
                        .ToArray()
                );

                return Ok(new
                {
                    roles = roleMap,
                    catalog = permisos.Select(p => new { id = p.Codigo, nombre = p.Nombre })
                });
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                // Fallback for environments where permission migration has not been applied yet.
                var fallbackRoles = BuildDefaultRolePermissions();
                var fallbackCatalog = new[]
                {
                    new { id = "crear_reporte", nombre = "Crear Reportes" },
                    new { id = "editar_reporte", nombre = "Editar Reportes" },
                    new { id = "eliminar_reporte", nombre = "Eliminar Reportes" },
                    new { id = "ver_dashboard", nombre = "Ver Dashboard" },
                    new { id = "gestionar_usuarios", nombre = "Gestionar Usuarios" },
                    new { id = "ver_reportes", nombre = "Ver Reportes otros usuarios" },
                    new { id = "generar_plan_ia", nombre = "Generar Plan de Acción con IA" },
                    new { id = "acceder_administracion", nombre = "Acceder a Administración" }
                };

                return Ok(new
                {
                    roles = fallbackRoles,
                    catalog = fallbackCatalog,
                    fallback = true
                });
            }
        }

        [HttpPut]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Save([FromBody] SaveRolePermissionsRequest req)
        {
            if (req?.Roles == null)
            {
                return BadRequest(new { message = "Payload de permisos inválido" });
            }

            try
            {
                var roles = await _db.Roles.ToListAsync();
                var permisos = await _db.Permisos.ToListAsync();

                var rolByKey = roles.ToDictionary(r => NormalizarRol(r.Nombre), r => r);
                var permisoByCode = permisos.ToDictionary(p => p.Codigo, p => p, StringComparer.OrdinalIgnoreCase);

                var allRolIds = roles.Select(r => r.Id).ToHashSet();
                var existing = await _db.RolesPermiso.Where(rp => allRolIds.Contains(rp.RolId)).ToListAsync();
                _db.RolesPermiso.RemoveRange(existing);

                foreach (var entry in req.Roles)
                {
                    if (!rolByKey.TryGetValue(entry.Key.ToUpperInvariant(), out var rol))
                    {
                        continue;
                    }

                    var codigos = entry.Value ?? Array.Empty<string>();
                    foreach (var codigo in codigos.Distinct(StringComparer.OrdinalIgnoreCase))
                    {
                        if (!permisoByCode.TryGetValue(codigo, out var permiso))
                        {
                            continue;
                        }

                        _db.RolesPermiso.Add(new RolPermiso
                        {
                            RolId = rol.Id,
                            PermisoId = permiso.Id
                        });
                    }
                }

                await _db.SaveChangesAsync();
                return Ok(new { success = true });
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                return StatusCode(409, new { success = false, message = "La tabla de permisos no existe. Aplique migraciones de base de datos (Permisos y RolesPermiso)." });
            }
        }

        private static string NormalizarRol(string nombreRol)
        {
            var value = (nombreRol ?? string.Empty).Trim().ToUpperInvariant();
            if (value.Contains("ADMIN")) return "ADMINISTRADOR";
            if (value.Contains("RESPONSABLE") || value.Contains("SST")) return "RESPONSABLE_SST";
            if (value.Contains("SUPERVISOR")) return "SUPERVISOR";
            if (value.Contains("OPERARIO") || value.Contains("COLABORADOR")) return "OPERARIO";
            return value;
        }

        public class SaveRolePermissionsRequest
        {
            public Dictionary<string, string[]> Roles { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        }

        private static Dictionary<string, string[]> BuildDefaultRolePermissions()
        {
            return new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
            {
                ["OPERARIO"] = new[] { "crear_reporte" },
                ["SUPERVISOR"] = new[] { "crear_reporte", "editar_reporte", "ver_dashboard", "ver_reportes" },
                ["RESPONSABLE_SST"] = new[] { "crear_reporte", "editar_reporte", "eliminar_reporte", "ver_dashboard", "ver_reportes", "generar_plan_ia" },
                ["ADMINISTRADOR"] = new[] { "crear_reporte", "editar_reporte", "eliminar_reporte", "ver_dashboard", "gestionar_usuarios", "ver_reportes", "generar_plan_ia", "acceder_administracion" }
            };
        }
    }
}
