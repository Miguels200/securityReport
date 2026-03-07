using System;
using MediatR;
using SecurityReport.Application.DTOs;

namespace SecurityReport.Application.Commands
{
    public record UpdateReportCommand(Guid Id, string Titulo, string Descripcion, Guid EstadoReporteId) : IRequest<ReportDto?>;
}