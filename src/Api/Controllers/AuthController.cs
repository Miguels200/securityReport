using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly SecurityReportDbContext _db;
        private readonly IPasswordHasherService _hasher;

        public AuthController(IConfiguration config, SecurityReportDbContext db, IPasswordHasherService hasher)
        {
            _config = config;
            _db = db;
            _hasher = hasher;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null) return Unauthorized();

            var res = _hasher.Verify(user.PasswordHash, req.Password);
            if (res != 1) return Unauthorized();

            var token = GenerateToken(user.Email, user.Rol?.Nombre ?? "Colaborador");
            return Ok(new { token });
        }

        private string GenerateToken(string email, string role)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(ClaimTypes.Role, role)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JWT_SECRET"] ?? string.Empty));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _config["JWT_ISSUER"],
                audience: _config["JWT_AUDIENCE"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }
    }
}