using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Infrastructure.Services
{
    public class NullServiceBusEnqueuer : IServiceBusEnqueuer
    {
        private readonly ILogger<NullServiceBusEnqueuer> _logger;

        public NullServiceBusEnqueuer(ILogger<NullServiceBusEnqueuer> logger)
        {
            _logger = logger;
        }

        public Task EnqueueAnalysisAsync(object payload)
        {
            _logger.LogWarning("Service Bus is not configured. Dropping enqueued analysis request.");
            return Task.CompletedTask;
        }
    }
}
