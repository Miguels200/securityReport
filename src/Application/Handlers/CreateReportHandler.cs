using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.DTOs;
using SecurityReport.Application.Interfaces;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Handlers
{
    public class CreateReportHandler : IRequestHandler<CreateReportCommand, ReportDto>
    {
        private readonly IReportRepository _repo;

        public CreateReportHandler(IReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<ReportDto> Handle(CreateReportCommand request, CancellationToken cancellationToken)
        {
            var r = new Reporte
            {
                Id = System.Guid.NewGuid(),
                Titulo = request.Titulo,
                Descripcion = request.Descripcion,
                AreaId = request.AreaId,
                EstadoReporteId = request.EstadoReporteId,
                ReportadoPorId = request.ReportadoPorId,
                FechaReporte = System.DateTime.UtcNow
            };

            await _repo.AddAsync(r);

            return new ReportDto(r.Id, r.Titulo, r.Descripcion, r.AreaId, r.EstadoReporteId, r.ReportadoPorId, r.FechaReporte);
        }
    }
}