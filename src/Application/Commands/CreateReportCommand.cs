using System;
using MediatR;
using SecurityReport.Application.DTOs;

namespace SecurityReport.Application.Commands
{
    public record CreateReportCommand(
        string Titulo,
        string Descripcion,
        Guid AreaId,
        Guid EstadoReporteId,
        Guid ReportadoPorId,
        Guid? TipoReporteId = null,
        int PersonasAfectadas = 1,
        bool TieneTestigos = false) : IRequest<ReportDto>;
}