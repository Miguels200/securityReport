using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.DTOs;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Application.Handlers
{
    public class UpdateReportHandler : IRequestHandler<UpdateReportCommand, ReportDto?>
    {
        private readonly IReportRepository _repo;

        public UpdateReportHandler(IReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<ReportDto?> Handle(UpdateReportCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repo.GetByIdAsync(request.Id);
            if (existing == null) return null;

            existing.Titulo = request.Titulo;
            existing.Descripcion = request.Descripcion;
            existing.Observaciones = request.Observaciones ?? string.Empty;
            existing.EstadoReporteId = request.EstadoReporteId;
            existing.UpdatedAt = System.DateTime.UtcNow;

            await _repo.UpdateAsync(existing);

            return new ReportDto(
                existing.Id,
                existing.Titulo,
                existing.Descripcion,
                existing.Observaciones,
                existing.AreaId,
                existing.EstadoReporteId,
                existing.ReportadoPorId,
                existing.FechaReporte,
                existing.PersonasAfectadas,
                existing.TieneTestigos,
                existing.PlanAccionJson);
        }
    }
}