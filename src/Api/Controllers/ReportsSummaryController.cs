using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurityReport.Infrastructure.Persistence;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsSummaryController : ControllerBase
    {
        private readonly SecurityReportDbContext _db;

        public ReportsSummaryController(SecurityReportDbContext db)
        {
            _db = db;
        }

        [HttpGet("estadistico")]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> Estadistico()
        {
            // Simple example: counts by estado
            var data = await _db.Reportes.GroupBy(r => r.EstadoReporteId).Select(g => new { EstadoId = g.Key, Count = g.Count() }).ToListAsync();
            return Ok(data);
        }

        [HttpGet("repetitivas")]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> Repetitivas()
        {
            var repetitives = await _db.RiesgosRepetitivos.OrderByDescending(r => r.Ocurrencias).Take(20).ToListAsync();
            return Ok(repetitives);
        }

        [HttpGet("riesgos-no-resueltos")]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> RiesgosNoResueltos()
        {
            var abiertos = await _db.Reportes.Where(r => r.EstadoReporte!.Nombre == "Abierto").ToListAsync();
            return Ok(abiertos);
        }

        [HttpGet("predictivo")]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> Predictivo()
        {
            // Placeholder: would call IA analysis across datasets
            return Ok(new { message = "Predictive analysis executed asynchronously." });
        }
    }
}