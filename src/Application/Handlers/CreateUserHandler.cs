using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.DTOs;
using SecurityReport.Domain.Entities;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Application.Handlers
{
    public class CreateUserHandler : IRequestHandler<CreateUserCommand, UserDto>
    {
        private readonly IUserRepository _repo;
        private readonly IPasswordHasherService _hasher;

        public CreateUserHandler(IUserRepository repo, IPasswordHasherService hasher)
        {
            _repo = repo;
            _hasher = hasher;
        }

        public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var user = new Usuario
            {
                Id = System.Guid.NewGuid(),
                Nombre = request.Nombre,
                Email = request.Email,
                PasswordHash = request.Password, // TEMPORAL: Sin hash para pruebas
                // PasswordHash = _hasher.Hash(request.Password),
                RolId = request.RolId,
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow
            };

            await _repo.AddAsync(user);

            return new UserDto(user.Id, user.Nombre, user.Email, string.Empty);
        }
    }
}