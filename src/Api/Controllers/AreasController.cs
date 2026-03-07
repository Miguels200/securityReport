using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using SecurityReport.Application.Commands;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AreasController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AreasController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateAreaCommand cmd)
        {
            var area = await _mediator.Send(cmd);
            return CreatedAtAction(nameof(Get), new { id = area.Id }, area);
        }

        [HttpGet("{id}")]
        public IActionResult Get(System.Guid id) => Ok();
    }
}