using System;

namespace SecurityReport.Application.DTOs
{
    public record ReportDto(
        Guid Id,
        string Titulo,
        string Descripcion,
        string Observaciones,
        Guid AreaId,
        Guid EstadoReporteId,
        Guid ReportadoPorId,
        DateTime FechaReporte,
        int PersonasAfectadas,
        bool TieneTestigos,
        string? PlanAccionJson);
}