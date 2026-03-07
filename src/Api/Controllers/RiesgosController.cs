using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SecurityReport.Application.Commands;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RiesgosController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RiesgosController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> Create([FromBody] CreateRiesgoCommand cmd)
        {
            var r = await _mediator.Send(cmd);
            return CreatedAtAction(nameof(Get), new { id = r.Id }, r);
        }

        [HttpGet("{id}")]
        public IActionResult Get(System.Guid id) => Ok();
    }
}