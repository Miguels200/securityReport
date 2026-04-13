using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;

namespace SecurityReport.Infrastructure.Services
{
    public class ServiceBusSenderWrapper : IServiceBusSender
    {
        private readonly ServiceBusSender _sender;

        public ServiceBusSenderWrapper(ServiceBusSender sender)
        {
            _sender = sender;
        }

        public Task SendMessageAsync(ServiceBusMessage message) => _sender.SendMessageAsync(message);
    }

    public class ServiceBusSenderFactory : IServiceBusSenderFactory
    {
        private readonly IServiceBusClientProvider _provider;

        public ServiceBusSenderFactory(IServiceBusClientProvider provider)
        {
            _provider = provider;
        }

        public IServiceBusSender CreateSender(string queueName)
        {
            var client = _provider.GetClient();
            var sender = client.CreateSender(queueName);
            return new ServiceBusSenderWrapper(sender);
        }
    }
}