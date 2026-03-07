using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Infrastructure.Services;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.Queries;

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

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Reportes.Include(r => r.Area).ToListAsync();
            return Ok(list);
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _mediator.Send(new GetReportByIdQuery(id));
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create([FromBody] CreateReporteRequest req)
        {
            var cmd = new CreateReportCommand(req.Titulo, req.Descripcion, req.AreaId, req.EstadoReporteId, req.ReportadoPorId);
            var created = await _mediator.Send(cmd);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateReporteRequest req)
        {
            var cmd = new UpdateReportCommand(id, req.Titulo, req.Descripcion, req.EstadoReporteId);
            var updated = await _mediator.Send(cmd);
            if (updated == null) return NotFound();
            return Ok(updated);
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

        public class CreateReporteRequest
        {
            public string Titulo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public Guid AreaId { get; set; }
            public Guid EstadoReporteId { get; set; }
            public Guid ReportadoPorId { get; set; }
        }

        public class UpdateReporteRequest
        {
            public string Titulo { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public Guid EstadoReporteId { get; set; }
        }
    }
}