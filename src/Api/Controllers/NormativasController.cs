using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SecurityReport.Application.Commands;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NormativasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public NormativasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> Create([FromBody] CreateNormativaCommand cmd)
        {
            var n = await _mediator.Send(cmd);
            return CreatedAtAction(nameof(Get), new { id = n.Id }, n);
        }

        [HttpGet("{id}")]
        public IActionResult Get(System.Guid id) => Ok();
    }
}