using System;
using MediatR;

namespace SecurityReport.Application.Commands
{
    public record DeleteReportCommand(Guid Id) : IRequest<bool>;
}