using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityReport.Application.Queries;
using SecurityReport.Application.DTOs;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Application.Handlers
{
    public class GetReportByIdHandler : IRequestHandler<GetReportByIdQuery, ReportDto?>
    {
        private readonly IReportRepository _repo;

        public GetReportByIdHandler(IReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<ReportDto?> Handle(GetReportByIdQuery request, CancellationToken cancellationToken)
        {
            var r = await _repo.GetByIdAsync(request.Id);
            if (r == null) return null;
            return new ReportDto(r.Id, r.Titulo, r.Descripcion, r.AreaId, r.EstadoReporteId, r.ReportadoPorId, r.FechaReporte);
        }
    }
}