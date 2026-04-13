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
                Observaciones = string.Empty,
                PersonasAfectadas = request.PersonasAfectadas,
                TieneTestigos = request.TieneTestigos,
                AreaId = request.AreaId,
                EstadoReporteId = request.EstadoReporteId,
                TipoReporteId = request.TipoReporteId,
                ReportadoPorId = request.ReportadoPorId,
                FechaReporte = System.DateTime.UtcNow,
                CreatedAt = System.DateTime.UtcNow,
                UpdatedAt = System.DateTime.UtcNow
            };

            await _repo.AddAsync(r);

            return new ReportDto(
                r.Id,
                r.Titulo,
                r.Descripcion,
                r.Observaciones,
                r.AreaId,
                r.EstadoReporteId,
                r.ReportadoPorId,
                r.FechaReporte,
                r.PersonasAfectadas,
                r.TieneTestigos,
                r.PlanAccionJson);
        }
    }
}