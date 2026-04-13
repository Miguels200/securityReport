using System;
using MediatR;
using SecurityReport.Application.DTOs;

namespace SecurityReport.Application.Queries
{
    public record GetReportByIdQuery(Guid Id) : IRequest<ReportDto?>;
}