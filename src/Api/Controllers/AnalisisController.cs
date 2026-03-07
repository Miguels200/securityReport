using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SecurityReport.Application.Commands;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalisisController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AnalisisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("trigger/{id}")]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> Trigger(System.Guid id)
        {
            var analysisId = await _mediator.Send(new TriggerIAAnalysisCommand(id, "analisis_general"));
            if (analysisId == System.Guid.Empty) return NotFound();
            return Ok(new { analysisId });
        }
    }
}