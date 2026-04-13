using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.Interfaces;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Handlers
{
    public class TriggerIAAnalysisHandler : IRequestHandler<TriggerIAAnalysisCommand, System.Guid>
    {
        private readonly IReportRepository _reportRepo;
        private readonly IAnalysisRepository _analysisRepo;
        private readonly IServiceBusEnqueuer _enqueuer;

        public TriggerIAAnalysisHandler(IReportRepository reportRepo, IAnalysisRepository analysisRepo, IServiceBusEnqueuer enqueuer)
        {
            _reportRepo = reportRepo;
            _analysisRepo = analysisRepo;
            _enqueuer = enqueuer;
        }

        public async Task<System.Guid> Handle(TriggerIAAnalysisCommand request, CancellationToken cancellationToken)
        {
            var report = await _reportRepo.GetByIdAsync(request.ReporteId);
            if (report == null) return System.Guid.Empty;

            var record = new AnalisisIA
            {
                Id = System.Guid.NewGuid(),
                ReporteId = request.ReporteId,
                Tipo = request.Tipo,
                Status = "Pending",
                CreatedAt = System.DateTime.UtcNow,
                AttemptCount = 0
            };

            await _analysisRepo.AddAsync(record);

            // Enqueue the work to Service Bus
            await _enqueuer.EnqueueAnalysisAsync(new { AnalysisId = record.Id });

            return record.Id;
        }
    }
}