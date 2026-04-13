using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Infrastructure.Persistence;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AreasController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SecurityReportDbContext _db;

        public AreasController(IMediator mediator, SecurityReportDbContext db)
        {
            _mediator = mediator;
            _db = db;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var areas = await _db.Areas
                .OrderBy(a => a.Nombre)
                .Select(a => new { id = a.Id, nombre = a.Nombre, descripcion = a.Descripcion })
                .ToListAsync();

            return Ok(areas);
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateAreaCommand cmd)
        {
            var area = await _mediator.Send(cmd);
            return CreatedAtAction(nameof(Get), new { id = area.Id }, area);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var area = await _db.Areas
                .Where(a => a.Id == id)
                .Select(a => new { id = a.Id, nombre = a.Nombre, descripcion = a.Descripcion })
                .FirstOrDefaultAsync();

            if (area == null) return NotFound();
            return Ok(area);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAreaRequest req)
        {
            var area = await _db.Areas.FirstOrDefaultAsync(a => a.Id == id);
            if (area == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(req.Nombre)) area.Nombre = req.Nombre;
            area.Descripcion = req.Descripcion;

            await _db.SaveChangesAsync();
            return Ok(new { id = area.Id, nombre = area.Nombre, descripcion = area.Descripcion });
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var area = await _db.Areas.FirstOrDefaultAsync(a => a.Id == id);
            if (area == null) return NotFound();

            _db.Areas.Remove(area);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        public class UpdateAreaRequest
        {
            public string? Nombre { get; set; }
            public string? Descripcion { get; set; }
        }
    }
}