using System;
using MediatR;
using SecurityReport.Application.DTOs;

namespace SecurityReport.Application.Commands
{
    public record UpdateReportCommand(Guid Id, string Titulo, string Descripcion, string Observaciones, Guid EstadoReporteId) : IRequest<ReportDto?>;
}