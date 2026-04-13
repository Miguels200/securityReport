using System;

namespace SecurityReport.Application.DTOs
{
    public record AreaDto(Guid Id, string Nombre, string? Descripcion);
}