using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Infrastructure.Background
{
    // Fallback local: solo se registra en Program.cs cuando SERVICEBUS_CONNECTION NO esta configurado.
    // Cuando Service Bus SI esta configurado, ServiceBusWorker es el unico consumidor (ver Program.cs),
    // evitando que ambos workers compitan por el mismo AnalisisIA.
    public class AIAnalysisWorker : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly AnalysisMessageHandler _messageHandler;
        private readonly ILogger<AIAnalysisWorker> _logger;

        public AIAnalysisWorker(IServiceProvider provider, AnalysisMessageHandler messageHandler, ILogger<AIAnalysisWorker> logger)
        {
            _provider = provider;
            _messageHandler = messageHandler;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AI Analysis Worker started (fallback local sin Service Bus).");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _provider.CreateScope();
                    var analysisRepo = scope.ServiceProvider.GetRequiredService<IAnalysisRepository>();

                    var pending = await analysisRepo.GetPendingAsync();
                    if (pending == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                        continue;
                    }

                    // Reutiliza exactamente la misma logica de reclamo/clasificacion/estados que ServiceBusWorker,
                    // sin duplicar el prompt ni el manejo de Origen/Failed.
                    await _messageHandler.HandleAsync(pending.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AIAnalysisWorker");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}