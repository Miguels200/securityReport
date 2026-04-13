using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.Interfaces;
using SecurityReport.Infrastructure.Persistence;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly SecurityReportDbContext _db;
        private readonly IPasswordHasherService _hasher;

        public UsersController(IMediator mediator, SecurityReportDbContext db, IPasswordHasherService hasher)
        {
            _mediator = mediator;
            _db = db;
            _hasher = hasher;
        }

        [HttpGet]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> GetAll()
        {
            var usuarios = await _db.Usuarios
                .Include(u => u.Rol)
                .OrderBy(u => u.Nombre)
                .Select(u => new
                {
                    id = u.Id,
                    nombre = u.Nombre,
                    email = u.Email,
                    rol = u.Rol != null ? u.Rol.Nombre : "Colaborador",
                    activo = true
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpPost]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Create([FromBody] CreateUserCommand cmd)
        {
            var user = await _mediator.Send(cmd);
            return CreatedAtAction(nameof(Get), new { id = user.Id }, user);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Get(Guid id)
        {
            var user = await _db.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    id = u.Id,
                    nombre = u.Nombre,
                    email = u.Email,
                    rol = u.Rol != null ? u.Rol.Nombre : "Colaborador",
                    activo = true
                })
                .FirstOrDefaultAsync();

            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest req)
        {
            var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(req.Nombre)) user.Nombre = req.Nombre;
            if (!string.IsNullOrWhiteSpace(req.Email)) user.Email = req.Email;
            if (req.RolId.HasValue) user.RolId = req.RolId.Value;
            user.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var result = await _db.Usuarios
                .Include(u => u.Rol)
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    id = u.Id,
                    nombre = u.Nombre,
                    email = u.Email,
                    rol = u.Rol != null ? u.Rol.Nombre : "Colaborador",
                    activo = true
                })
                .FirstOrDefaultAsync();

            return Ok(result);
        }

        [HttpPost("{id}/resetear-contraseña")]
        [Authorize(Policy = "RequireAdmin")]
        public async Task<IActionResult> ResetPassword(Guid id)
        {
            var user = await _db.Usuarios.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            user.PasswordHash = "Temporal123!"; // TEMPORAL: Sin hash
            // user.PasswordHash = _hasher.Hash("Temporal123!");
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { message = "Contraseña reseteada", temporalPassword = "Temporal123!" });
        }

        public class UpdateUserRequest
        {
            public string? Nombre { get; set; }
            public string? Email { get; set; }
            public Guid? RolId { get; set; }
            public bool? Activo { get; set; }
        }
    }
}