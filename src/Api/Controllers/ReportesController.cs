using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Infrastructure.Services;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.Queries;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportesController : ControllerBase
    {
        private readonly SecurityReportDbContext _db;
        private readonly IAIAnalysisService _ai;
        private readonly IBlobStorageService _blob;
        private readonly IMediator _mediator;

        public ReportesController(SecurityReportDbContext db, IAIAnalysisService ai, IBlobStorageService blob, IMediator mediator)
        {
            _db = db;
            _ai = ai;
            _blob = blob;
            _mediator = mediator;
        }

        [HttpGet("tipos")]
        [Authorize]
        public async Task<IActionResult> GetTipos()
        {
            var tipos = await _db.TiposReporte
                .AsNoTracking()
                .Select(t => new { t.Id, t.Nombre, t.Descripcion })
                .ToListAsync();
            return Ok(tipos);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var list = await _db.Reportes
                    .Include(r => r.Area)
                    .Include(r => r.EstadoReporte)
                    .Include(r => r.TipoReporte)
                    .Include(r => r.ReportadoPor)
                    .Include(r => r.Condiciones)
                    .Include(r => r.Actos)
                    .Include(r => r.Analisis)
                    .AsNoTracking()
                    .ToListAsync();

                var dtos = list.Select(r => new
                {
                    r.Id,
                    r.Titulo,
                    r.Descripcion,
                    r.FechaReporte,
                    r.CreatedAt,
                    Ubicacion = ExtraerUbicacionDesdeDescripcion(r.Descripcion),
                    Area = new
                    {
                        Id = r.AreaId,
                        Nombre = r.Area?.Nombre ?? "N/A"
                    },
                    AreaNombre = r.Area?.Nombre ?? "N/A",
                    Estado = NormalizarEstadoParaFrontend(r.EstadoReporte?.Nombre),
                    EstadoOriginal = r.EstadoReporte?.Nombre ?? "Desconocido",
                    TipoReporte = r.TipoReporte == null ? null : new
                    {
                        Id = r.TipoReporteId,
                        r.TipoReporte.Nombre
                    },
                    TieneTestigos = r.TieneTestigos,
                    PersonasAfectadas = r.PersonasAfectadas,
                    TienePlanAccion = !string.IsNullOrWhiteSpace(r.PlanAccionJson),
                    NivelRiesgo = DeterminarNivelRiesgo(r),
                    ReportadoPor = r.ReportadoPor?.Nombre ?? "Anónimo",
                    ReportadoPorId = r.ReportadoPorId,
                    ReportadoPorEmail = r.ReportadoPor?.Email ?? string.Empty,
                    r.AreaId,
                    r.EstadoReporteId
                }).ToList();

                return Ok(dtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR en GetAll: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(Guid id)
        {
            var reporte = await _db.Reportes
                .Include(r => r.Area)
                .Include(r => r.EstadoReporte)
                .Include(r => r.TipoReporte)
                .Include(r => r.ReportadoPor)
                .Include(r => r.Evidencias)
                .Include(r => r.Condiciones)
                .Include(r => r.Actos)
                .Include(r => r.Analisis)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reporte == null) return NotFound();

            var condicion = reporte.Condiciones?.FirstOrDefault()?.Descripcion
                ?? reporte.Actos?.FirstOrDefault()?.Descripcion
                ?? "Sin condición registrada";

            var nivelRiesgo = DeterminarNivelRiesgo(reporte);

            var dto = new
            {
                reporte.Id,
                reporte.Titulo,
                reporte.Descripcion,
                Ubicacion = ExtraerUbicacionDesdeDescripcion(reporte.Descripcion),
                Area = new
                {
                    Id = reporte.AreaId,
                    Nombre = reporte.Area?.Nombre ?? "N/A"
                },
                Condicion = new
                {
                    Id = Guid.Empty,
                    Nombre = condicion,
                    Descripcion = condicion
                },
                NivelRiesgo = nivelRiesgo,
                Estado = reporte.EstadoReporte?.Nombre ?? "Desconocido",
                TipoReporte = reporte.TipoReporte == null ? null : new
                {
                    Id = reporte.TipoReporteId,
                    reporte.TipoReporte.Nombre
                },
                TieneTestigos = reporte.TieneTestigos,
                PersonasAfectadas = reporte.PersonasAfectadas,
                PlanAccion = DeserializarPlanAccion(reporte.PlanAccionJson),
                reporte.FechaReporte,
                ReportadoPor = new
                {
                    Id = reporte.ReportadoPorId,
                    Nombre = reporte.ReportadoPor?.Nombre ?? "Anónimo",
                    Email = reporte.ReportadoPor?.Email ?? string.Empty
                },
                AsignadoA = (object?)null,
                Evidencias = reporte.Evidencias?.Select(e => (object)new
                {
                    e.Id,
                    Tipo = e.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase) ? "IMAGEN" : "DOCUMENTO",
                    Nombre = e.FileName,
                    Url = e.BlobUrl,
                    FechaCarga = e.UploadedAt
                }).ToList() ?? new List<object>(),
                Observaciones = string.IsNullOrWhiteSpace(reporte.Observaciones)
                    ? ExtraerObservacionesDesdeDescripcion(reporte.Descripcion)
                    : reporte.Observaciones
            };

            return Ok(dto);
        }

        private static string DeterminarNivelRiesgo(Reporte reporte)
        {
            var textos = new List<string>();

            if (!string.IsNullOrWhiteSpace(reporte.Descripcion))
            {
                textos.Add(reporte.Descripcion);
            }

            if (reporte.Condiciones != null)
            {
                textos.AddRange(
                    reporte.Condiciones
                        .Where(c => !string.IsNullOrWhiteSpace(c.Descripcion))
                        .Select(c => c.Descripcion)
                );
            }

            if (reporte.Actos != null)
            {
                textos.AddRange(
                    reporte.Actos
                        .Where(a => !string.IsNullOrWhiteSpace(a.Descripcion))
                        .Select(a => a.Descripcion)
                );
            }

            if (reporte.Analisis != null)
            {
                textos.AddRange(
                    reporte.Analisis
                        .Where(a => !string.IsNullOrWhiteSpace(a.ResultadoJson))
                        .Select(a => a.ResultadoJson)
                );
            }

            var contenido = string.Join(" ", textos).ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(contenido))
            {
                return "MEDIO";
            }

            if (ContieneAlguno(
                contenido,
                "critico", "crítico", "explosion", "explosión", "incendio", "alta tension", "alta tensión", "electroc", "asfixia"
            ))
            {
                return "CRÍTICO";
            }

            if (ContieneAlguno(
                contenido,
                "alto", "caida", "caída", "sin arnes", "sin arnés", "quimic", "químic", "fuga", "atrap", "golpe", "cort"
            ))
            {
                return "ALTO";
            }

            if (ContieneAlguno(
                contenido,
                "medio", "resbal", "obstacul", "obstáculo", "ruido", "ergonom", "iluminacion", "iluminación", "senalizacion", "señalización"
            ))
            {
                return "MEDIO";
            }

            if (ContieneAlguno(contenido, "bajo", "leve", "menor"))
            {
                return "BAJO";
            }

            var tieneHallazgos =
                (reporte.Condiciones != null && reporte.Condiciones.Any()) ||
                (reporte.Actos != null && reporte.Actos.Any());

            return tieneHallazgos ? "MEDIO" : "BAJO";
        }

        private static string NormalizarEstadoParaFrontend(string? estadoNombre)
        {
            if (string.IsNullOrWhiteSpace(estadoNombre))
            {
                return "NUEVO";
            }

            var estado = estadoNombre.Trim().ToLowerInvariant();

            if (estado.Contains("abierto") || estado.Contains("nuevo")) return "NUEVO";
            if (estado.Contains("revisi")) return "EN_REVISIÓN";
            if (estado.Contains("asign")) return "ASIGNADO";
            if (estado.Contains("corre")) return "EN_CORRECCIÓN";
            if (estado.Contains("resuelt")) return "RESUELTO";
            if (estado.Contains("cerr")) return "CERRADO";
            if (estado.Contains("rechaz")) return "RECHAZADO";

            return estadoNombre.ToUpperInvariant().Replace(" ", "_");
        }

        private static bool ContieneAlguno(string texto, params string[] palabras)
        {
            return palabras.Any(p => texto.Contains(p, StringComparison.OrdinalIgnoreCase));
        }

        private static string ExtraerObservacionesDesdeDescripcion(string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                return string.Empty;
            }

            var lineas = descripcion
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(l => l.Trim())
                .ToList();

            var observacionLinea = lineas.FirstOrDefault(l =>
                l.StartsWith("Observaciones:", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(observacionLinea))
            {
                return string.Empty;
            }

            var idx = observacionLinea.IndexOf(':');
            if (idx < 0 || idx >= observacionLinea.Length - 1)
            {
                return string.Empty;
            }

            return observacionLinea[(idx + 1)..].Trim();
        }

        private static string ExtraerUbicacionDesdeDescripcion(string? descripcion)
        {
            if (string.IsNullOrWhiteSpace(descripcion))
            {
                return "No especificada";
            }

            var lineas = descripcion
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)
                .Select(l => l.Trim())
                .ToList();

            var ubicacionLinea = lineas.FirstOrDefault(l =>
                l.StartsWith("Ubicacion:", StringComparison.OrdinalIgnoreCase)
                || l.StartsWith("Ubicación:", StringComparison.OrdinalIgnoreCase));

            if (string.IsNullOrWhiteSpace(ubicacionLinea))
            {
                return "No especificada";
            }

            var idx = ubicacionLinea.IndexOf(':');
            if (idx < 0 || idx >= ubicacionLinea.Length - 1)
            {
                return "No especificada";
            }

            var valor = ubicacionLinea[(idx + 1)..].Trim();
            return string.IsNullOrWhiteSpace(valor) ? "No especificada" : valor;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReporteRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Titulo) || string.IsNullOrWhiteSpace(req.Descripcion))
            {
                return BadRequest(new { message = "Título y descripción son obligatorios" });
            }
            if (req.AreaId == Guid.Empty)
            {
                return BadRequest(new { message = "Área inválida" });
            }

            var areaExiste = await _db.Areas.AnyAsync(a => a.Id == req.AreaId);
            if (!areaExiste)
            {
                return BadRequest(new { message = "El área seleccionada no existe" });
            }

            var emailUsuario =
                User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                User.FindFirstValue(ClaimTypes.Email) ??
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(emailUsuario))
            {
                return Unauthorized();
            }

            var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == emailUsuario);
            if (usuario == null)
            {
                return Unauthorized();
            }

            var estadoReporteId = req.EstadoReporteId;
            if (estadoReporteId == Guid.Empty)
            {
                estadoReporteId = await _db.EstadosReporte
                    .Where(e => e.Nombre == "Abierto")
                    .Select(e => e.Id)
                    .FirstOrDefaultAsync();
            }

            if (estadoReporteId == Guid.Empty)
            {
                return BadRequest(new { message = "No se encontró estado de reporte inicial" });
            }

            var cmd = new CreateReportCommand(
                req.Titulo,
                req.Descripcion,
                req.AreaId,
                estadoReporteId,
                usuario.Id,
                req.TipoReporteId,
                req.PersonasAfectadas,
                req.TieneTestigos);
            var created = await _mediator.Send(cmd);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReporteRequest req)
        {
            var existente = await _db.Reportes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
            if (existente == null) return NotFound();

            if (req == null)
            {
                return BadRequest(new { message = "Payload inválido" });
            }

            var titulo = string.IsNullOrWhiteSpace(req.Titulo) ? existente.Titulo : req.Titulo;
            var descripcion = string.IsNullOrWhiteSpace(req.Descripcion) ? existente.Descripcion : req.Descripcion;
            var observaciones = req.Observaciones ?? existente.Observaciones ?? string.Empty;
            var estadoReporteId = req.EstadoReporteId == Guid.Empty ? existente.EstadoReporteId : req.EstadoReporteId;

            var cmd = new UpdateReportCommand(id, titulo, descripcion, observaciones, estadoReporteId);
            var updated = await _mediator.Send(cmd);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpPatch("{id}/observaciones")]
        [Authorize]
        public async Task<IActionResult> GuardarObservaciones(Guid id, [FromBody] GuardarObservacionesRequest req)
        {
            var reporte = await _db.Reportes.FirstOrDefaultAsync(r => r.Id == id);
            if (reporte == null) return NotFound();

            var observaciones = (req?.Observaciones ?? string.Empty).Trim();
            reporte.Observaciones = observaciones;
            reporte.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(new
            {
                id = reporte.Id,
                observaciones,
                descripcion = reporte.Descripcion
            });
        }

        [HttpPatch("{id}/estado")]
        [Authorize]
        public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] CambiarEstadoRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.EstadoNombre))
            {
                return BadRequest(new { message = "Debe enviar un estado válido" });
            }

            var reporte = await _db.Reportes.FirstOrDefaultAsync(r => r.Id == id);
            if (reporte == null) return NotFound();

            var estadoSolicitado = NormalizarTextoComparable(req.EstadoNombre);
            var estados = await _db.EstadosReporte.ToListAsync();

            var estado = estados.FirstOrDefault(e =>
            {
                var nombreEstadoDb = NormalizarTextoComparable(e.Nombre);

                if (nombreEstadoDb == estadoSolicitado)
                {
                    return true;
                }

                // Equivalencias entre nombres de UI y nombres de BD
                if (estadoSolicitado == "nuevo" && nombreEstadoDb.Contains("abierto")) return true;
                if (estadoSolicitado == "enrevision" && nombreEstadoDb.Contains("revision")) return true;
                if (estadoSolicitado == "asignado" && nombreEstadoDb.Contains("asign")) return true;
                if (estadoSolicitado == "encorreccion" && nombreEstadoDb.Contains("corre")) return true;
                if (estadoSolicitado == "resuelto" && nombreEstadoDb.Contains("resuelt")) return true;
                if (estadoSolicitado == "cerrado" && nombreEstadoDb.Contains("cerr")) return true;
                if (estadoSolicitado == "rechazado" && nombreEstadoDb.Contains("rechaz")) return true;

                return false;
            });

            if (estado == null)
                return BadRequest(new { message = $"Estado '{req.EstadoNombre}' no encontrado en la base de datos" });

            reporte.EstadoReporteId = estado.Id;
            await _db.SaveChangesAsync();

            return Ok(new { id = reporte.Id, estado = estado.Nombre, estadoId = estado.Id });
        }

        private static string NormalizarTextoComparable(string? valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                return string.Empty;
            }

            var texto = valor.Trim().Replace("_", " ").ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in texto)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark && char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        // ─── Plan de Acción IA ─────────────────────────────────────────────────────

        [HttpPost("{id}/plan-accion")]
        [Authorize]
        public async Task<IActionResult> GenerarPlanAccion(Guid id)
        {
            if (!await UserHasPermission("generar_plan_ia"))
            {
                return Forbid();
            }

            var reporte = await _db.Reportes
                .Include(r => r.Area)
                .Include(r => r.TipoReporte)
                .Include(r => r.EstadoReporte)
                .Include(r => r.Condiciones)
                .Include(r => r.Actos)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reporte == null) return NotFound();

            var condicion = reporte.Condiciones?.FirstOrDefault()?.Descripcion
                ?? reporte.Actos?.FirstOrDefault()?.Descripcion
                ?? reporte.Descripcion
                ?? "Sin condición registrada";

            var request = new PlanAccionRequest
            {
                ReporteId       = reporte.Id,
                Titulo          = reporte.Titulo,
                Descripcion     = reporte.Descripcion ?? string.Empty,
                TipoReporte     = reporte.TipoReporte?.Nombre ?? "No especificado",
                NivelRiesgo     = DeterminarNivelRiesgo(reporte),
                Area            = reporte.Area?.Nombre ?? "No especificada",
                Ubicacion       = ExtraerUbicacionDesdeDescripcion(reporte.Descripcion),
                Condicion       = condicion,
                Estado          = reporte.EstadoReporte?.Nombre ?? "Desconocido",
                PersonasAfectadas = reporte.PersonasAfectadas,
                TieneTestigos   = reporte.TieneTestigos
            };

            var plan = await _ai.GeneratePlanAccionAsync(request);

            var reporteEditable = await _db.Reportes.FirstOrDefaultAsync(r => r.Id == id);
            if (reporteEditable != null)
            {
                reporteEditable.PlanAccionJson = JsonSerializer.Serialize(plan);
                reporteEditable.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }

            return Ok(plan);
        }

        [HttpPatch("{id}/plan-accion")]
        [Authorize]
        public async Task<IActionResult> GuardarPlanAccion(Guid id, [FromBody] GuardarPlanAccionRequest req)
        {
            if (!await UserHasPermission("generar_plan_ia"))
            {
                return Forbid();
            }

            var reporte = await _db.Reportes.FirstOrDefaultAsync(r => r.Id == id);
            if (reporte == null) return NotFound();

            if (req?.PlanAccion == null)
            {
                return BadRequest(new { message = "Debe enviar un plan de acción válido" });
            }

            reporte.PlanAccionJson = JsonSerializer.Serialize(req.PlanAccion);
            reporte.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(req.PlanAccion);
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteReportCommand(id));
            if (!result) return NotFound();
            return NoContent();
        }

        [HttpPost("trigger-analysis/{id}")]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> TriggerAnalysis(Guid id)
        {
            var report = await _db.Reportes.FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();

            // create AnalisisIA record and enqueue background job
            var analysisId = await _mediator.Send(new TriggerIAAnalysisCommand(id, "analisis_general"));

            return Ok(new { analysisId });
        }

        [HttpPost("{id}/evidencia")]
        [Authorize]
        public async Task<IActionResult> UploadEvidence(Guid id)
        {
            var report = await _db.Reportes.FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();

            var file = Request.Form.Files.GetFile("file");
            if (file == null) return BadRequest("file missing");

            var fileName = $"{id}/{Guid.NewGuid()}_{file.FileName}";
            using var stream = file.OpenReadStream();
            var url = await _blob.UploadAsync("evidencias", fileName, stream, file.ContentType);

            var ev = new Domain.Entities.Evidencia
            {
                Id = Guid.NewGuid(),
                FileName = file.FileName,
                ContentType = file.ContentType,
                Size = file.Length,
                BlobUrl = url,
                ReporteId = id,
                UploadedAt = DateTime.UtcNow
            };

            _db.Evidencias.Add(ev);
            await _db.SaveChangesAsync();

            return Ok(ev);
        }

        private async Task<bool> UserHasPermission(string permissionCode)
        {
            var roles = User.FindAll(ClaimTypes.Role)
                .Select(r => NormalizeRole(r.Value))
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (roles.Length == 0)
            {
                return false;
            }

            try
            {
                var dbRoles = await _db.Roles.AsNoTracking().ToListAsync();
                var normalizedRoleIds = dbRoles
                    .Where(r => roles.Contains(NormalizeRole(r.Nombre), StringComparer.OrdinalIgnoreCase))
                    .Select(r => r.Id)
                    .ToHashSet();

                if (normalizedRoleIds.Count == 0)
                {
                    return false;
                }

                return await _db.RolesPermiso
                    .AsNoTracking()
                    .Where(rp => normalizedRoleIds.Contains(rp.RolId))
                    .Join(_db.Permisos.AsNoTracking(), rp => rp.PermisoId, p => p.Id, (_, p) => p.Codigo)
                    .AnyAsync(c => c == permissionCode);
            }
            catch (SqlException ex) when (ex.Number == 208)
            {
                var fallback = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
                {
                    ["OPERARIO"] = new[] { "crear_reporte" },
                    ["SUPERVISOR"] = new[] { "crear_reporte", "editar_reporte", "ver_dashboard", "ver_reportes" },
                    ["RESPONSABLE_SST"] = new[] { "crear_reporte", "editar_reporte", "eliminar_reporte", "ver_dashboard", "ver_reportes", "generar_plan_ia" },
                    ["ADMINISTRADOR"] = new[] { "crear_reporte", "editar_reporte", "eliminar_reporte", "ver_dashboard", "gestionar_usuarios", "ver_reportes", "generar_plan_ia", "acceder_administracion" }
                };

                return roles.Any(role =>
                    fallback.TryGetValue(role, out var permissions)
                    && permissions.Contains(permissionCode, StringComparer.OrdinalIgnoreCase));
            }
        }

        private static string NormalizeRole(string role)
        {
            var value = (role ?? string.Empty).Trim().ToUpperInvariant();
            if (value.Contains("ADMIN")) return "ADMINISTRADOR";
            if (value.Contains("RESPONSABLE") || value.Contains("SST")) return "RESPONSABLE_SST";
            if (value.Contains("SUPERVISOR")) return "SUPERVISOR";
            if (value.Contains("OPERARIO") || value.Contains("COLABORADOR")) return "OPERARIO";
            return value;
        }

        public class CreateReporteRequest
        {
            public string Titulo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public Guid AreaId { get; set; }
            public Guid EstadoReporteId { get; set; }
            public Guid? TipoReporteId { get; set; }
            public int PersonasAfectadas { get; set; } = 1;
            public bool TieneTestigos { get; set; }
        }

        public class UpdateReporteRequest
        {
            public string Titulo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public string Observaciones { get; set; } = string.Empty;
            public Guid EstadoReporteId { get; set; }
        }

        public class CambiarEstadoRequest
        {
            public string EstadoNombre { get; set; } = string.Empty;
        }

        public class GuardarObservacionesRequest
        {
            public string Observaciones { get; set; } = string.Empty;
        }

        public class GuardarPlanAccionRequest
        {
            public PlanAccionIAResult? PlanAccion { get; set; }
        }

        private static PlanAccionIAResult? DeserializarPlanAccion(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var directo = JsonSerializer.Deserialize<PlanAccionIAResult>(json, options);
                if (directo != null)
                {
                    return directo;
                }

                // Compatibilidad con payload histórico doble-serializado (JSON string)
                var asString = JsonSerializer.Deserialize<string>(json, options);
                if (string.IsNullOrWhiteSpace(asString))
                {
                    return null;
                }

                return JsonSerializer.Deserialize<PlanAccionIAResult>(asString, options);
            }
            catch
            {
                return null;
            }
        }
    }
}