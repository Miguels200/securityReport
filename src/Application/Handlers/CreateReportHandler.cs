using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using SecurityReport.Application.Commands;
using SecurityReport.Application.Common;
using SecurityReport.Application.DTOs;
using SecurityReport.Application.Interfaces;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Handlers
{
    public class CreateReportHandler : IRequestHandler<CreateReportCommand, ReportDto>
    {
        private readonly IReportRepository _repo;
        private readonly IMediator _mediator;
        private readonly ILogger<CreateReportHandler> _logger;

        public CreateReportHandler(IReportRepository repo, IMediator mediator, ILogger<CreateReportHandler> logger)
        {
            _repo = repo;
            _mediator = mediator;
            _logger = logger;
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

            // El reporte debe quedar guardado sin importar lo que ocurra despues con el analisis IA.
            await _repo.AddAsync(r);

            // Disparo automatico del analisis (Objetivo 3 de la tesis). Nunca debe hacer fallar la
            // creacion del reporte: se captura cualquier error de encolado/creacion de la solicitud.
            System.Guid? analysisId = null;
            try
            {
                var triggeredId = await _mediator.Send(new TriggerIAAnalysisCommand(r.Id, AnalisisTipos.ClasificacionRiesgo), cancellationToken);
                analysisId = triggeredId == System.Guid.Empty ? null : triggeredId;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "No se pudo iniciar el analisis IA automatico para el reporte {ReporteId}. El reporte fue creado correctamente.", r.Id);
            }

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
                r.PlanAccionJson,
                analysisId);
        }
    }
}