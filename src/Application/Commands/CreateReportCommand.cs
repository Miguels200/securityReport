using System;
using MediatR;
using SecurityReport.Application.DTOs;

namespace SecurityReport.Application.Commands
{
    public record CreateReportCommand(string Titulo, string Descripcion, Guid AreaId, Guid EstadoReporteId, Guid ReportadoPorId) : IRequest<ReportDto>;
}