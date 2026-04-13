using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SecurityReport.Application.Interfaces;
using SecurityReport.Infrastructure.Services;

namespace SecurityReport.Infrastructure.Background
{
    public class ServiceBusWorker : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly IServiceBusClientProvider _clientProvider;
        private readonly IConfiguration _config;
        private readonly ILogger<ServiceBusWorker> _logger;
        private ServiceBusProcessor? _processor;
        private readonly AnalysisMessageHandler _messageHandler;

        public ServiceBusWorker(IServiceProvider provider, IServiceBusClientProvider clientProvider, IConfiguration config, ILogger<ServiceBusWorker> logger, AnalysisMessageHandler messageHandler)
        {
            _provider = provider;
            _clientProvider = clientProvider;
            _config = config;
            _logger = logger;
            _messageHandler = messageHandler;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            var client = _clientProvider.GetClient();
            var queue = _config["SERVICEBUS_QUEUE_ANALYSIS"] ?? "analysis-queue";
            _processor = client.CreateProcessor(queue, new ServiceBusProcessorOptions { MaxConcurrentCalls = 2, AutoCompleteMessages = false });
            _processor.ProcessMessageAsync += ProcessMessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;
            return _processor.StartProcessingAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            if (_processor != null)
            {
                _processor.ProcessMessageAsync -= ProcessMessageHandler;
                _processor.ProcessErrorAsync -= ErrorHandler;
                return _processor.StopProcessingAsync(cancellationToken);
            }

            return Task.CompletedTask;
        }

        public async Task ProcessMessageHandler(ProcessMessageEventArgs args)
        {
            var body = args.Message.Body.ToString();
            try
            {
                var payload = JsonSerializer.Deserialize<JsonElement>(body);
                if (payload.TryGetProperty("AnalysisId", out var aid))
                {
                    var analysisId = aid.GetGuid();

                    // Delegate processing to handler (testable)
                    await _messageHandler.HandleAsync(analysisId);

                    await args.CompleteMessageAsync(args.Message);
                }
                else
                {
                    await args.DeadLetterMessageAsync(args.Message, "InvalidPayload", "Missing AnalysisId");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing service bus message");
                // Let the message be retried by not completing it; if max delivery attempts reached, it will go to DLQ
            }
        }

        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "ServiceBus error when processing {EntityPath}", args.EntityPath);
            return Task.CompletedTask;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken) => Task.CompletedTask;
    }
}