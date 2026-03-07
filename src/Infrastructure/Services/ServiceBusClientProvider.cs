using System;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;

namespace SecurityReport.Infrastructure.Services
{
    public interface IServiceBusClientProvider
    {
        ServiceBusClient GetClient();
    }

    public class ServiceBusClientProvider : IServiceBusClientProvider
    {
        private readonly ServiceBusClient? _client;

        public ServiceBusClientProvider(IConfiguration configuration)
        {
            var conn = configuration["SERVICEBUS_CONNECTION"];
            if (!string.IsNullOrWhiteSpace(conn))
            {
                _client = new ServiceBusClient(conn);
            }
            else
            {
                // Do not throw here to avoid DI construction failure during host build.
                // Defer throwing to GetClient() so the app can start and report a clearer error when Service Bus is used.
                _client = null;
            }
        }

        public ServiceBusClient GetClient()
        {
            if (_client == null)
                throw new InvalidOperationException("SERVICEBUS_CONNECTION is not configured. Set the SERVICEBUS_CONNECTION environment variable or configuration.");

            return _client;
        }
    }
}