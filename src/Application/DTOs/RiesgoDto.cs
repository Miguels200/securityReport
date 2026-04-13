using System;

namespace SecurityReport.Application.DTOs
{
    public record RiesgoDto(Guid Id, string Descripcion, int Ocurrencias, string NivelRiesgo, DateTime FirstDetected, DateTime LastDetected);
}