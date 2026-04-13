using System;

namespace SecurityReport.Application.DTOs
{
    public record UserDto(Guid Id, string Nombre, string Email, string Rol);
}