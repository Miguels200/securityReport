using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace SecurityReport.Infrastructure.Services
{
    public interface IServiceBusSender
    {
        Task SendMessageAsync(ServiceBusMessage message);
    }

    public interface IServiceBusSenderFactory
    {
        IServiceBusSender CreateSender(string queueName);
    }
}