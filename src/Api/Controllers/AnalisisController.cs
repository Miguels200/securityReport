using System;
using System.Linq;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.DTOs;
using SecurityReport.Infrastructure.Persistence;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalisisController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SecurityReportDbContext _db;

        public AnalisisController(IMediator mediator, SecurityReportDbContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        [HttpPost("trigger/{id}")]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> Trigger(System.Guid id)
        {
            var analysisId = await _mediator.Send(new TriggerIAAnalysisCommand(id, "analisis_general"));
            if (analysisId == System.Guid.Empty) return NotFound();
            return Ok(new { analysisId });
        }

        // Autorizacion: ResponsableSST/Administrador pueden consultar cualquier analisis (mismas
        // politicas ya usadas en el resto de la API). Un usuario sin ese rol solo puede consultar el
        // analisis si el reporte asociado le pertenece (ReportadoPorId == usuario autenticado).
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(Guid id)
        {
            var analisis = await _db.Analisis.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            if (analisis == null) return NotFound();

            var reporte = await _db.Reportes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == analisis.ReporteId);
            if (reporte == null) return NotFound();

            var esResponsableOAdmin = User.IsInRole("ResponsableSST") || User.IsInRole("Administrador");
            if (!esResponsableOAdmin)
            {
                var emailUsuario =
                    User.FindFirstValue(JwtRegisteredClaimNames.Sub) ??
                    User.FindFirstValue(ClaimTypes.Email) ??
                    User.FindFirstValue(ClaimTypes.NameIdentifier);

                var usuario = string.IsNullOrWhiteSpace(emailUsuario)
                    ? null
                    : await _db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Email == emailUsuario);

                if (usuario == null || usuario.Id != reporte.ReportadoPorId)
                {
                    return Forbid();
                }
            }

            var recomendaciones = string.IsNullOrWhiteSpace(analisis.RecomendacionesJson)
                ? new System.Collections.Generic.List<string>()
                : JsonSerializer.Deserialize<System.Collections.Generic.List<string>>(analisis.RecomendacionesJson) ?? new System.Collections.Generic.List<string>();

            // Mensaje generico para el usuario final; el detalle tecnico solo queda en ErrorMensaje/logs.
            var mensajeError = analisis.Status == "Failed"
                ? "No fue posible completar el análisis del reporte. Intente nuevamente o contacte al responsable del sistema."
                : null;

            var dto = new AnalisisDto(
                analisis.Id,
                analisis.ReporteId,
                analisis.Status,
                analisis.TipoRiesgo?.ToString(),
                analisis.NivelRiesgo?.ToString(),
                analisis.Prioridad?.ToString(),
                analisis.Justificacion,
                recomendaciones,
                analisis.Origen?.ToString(),
                analisis.StartedAt,
                analisis.CompletedAt,
                analisis.CreatedAt,
                mensajeError);

            return Ok(dto);
        }
    }
}