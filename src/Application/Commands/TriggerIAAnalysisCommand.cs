using System;
using MediatR;

namespace SecurityReport.Application.Commands
{
    public record TriggerIAAnalysisCommand(Guid ReporteId, string Tipo) : IRequest<Guid>;
}