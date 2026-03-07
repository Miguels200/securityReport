using System;
using MediatR;
using SecurityReport.Application.DTOs;

namespace SecurityReport.Application.Commands
{
    public record CreateUserCommand(string Nombre, string Email, string Password, Guid RolId) : IRequest<UserDto>;
}