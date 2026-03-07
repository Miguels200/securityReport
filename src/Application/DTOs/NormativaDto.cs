using System;

namespace SecurityReport.Application.DTOs
{
    public record NormativaDto(Guid Id, string Codigo, string Titulo, string Contenido);
}