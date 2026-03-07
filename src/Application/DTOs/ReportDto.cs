using System;

namespace SecurityReport.Application.DTOs
{
    public record ReportDto(Guid Id, string Titulo, string Descripcion, Guid AreaId, Guid EstadoReporteId, Guid ReportadoPorId, DateTime FechaReporte);
}