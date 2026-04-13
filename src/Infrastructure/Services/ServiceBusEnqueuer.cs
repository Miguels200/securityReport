using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Infrastructure.Services
{
    public class ServiceBusEnqueuer : IServiceBusEnqueuer
    {
        private readonly IServiceBusSenderFactory _senderFactory;
        private readonly string _queueName;

        public ServiceBusEnqueuer(IServiceBusSenderFactory senderFactory, IConfiguration config)
        {
            _senderFactory = senderFactory;
            _queueName = config["SERVICEBUS_QUEUE_ANALYSIS"] ?? "analysis-queue";
        }

        public async Task EnqueueAnalysisAsync(object payload)
        {
            var sender = _senderFactory.CreateSender(_queueName);
            var msg = new ServiceBusMessage(JsonSerializer.Serialize(payload))
            {
                ContentType = "application/json"
            };
            await sender.SendMessageAsync(msg);
        }
    }
}