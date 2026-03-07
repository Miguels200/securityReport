using System.Text.Json;
using System.Threading.Tasks;
using Moq;
using Xunit;
using Azure.Messaging.ServiceBus;
using SecurityReport.Infrastructure.Services;
using Microsoft.Extensions.Configuration;

namespace Tests.Unit
{
    public class ServiceBusEnqueuerTests
    {
        [Fact]
        public async Task EnqueueAnalysisAsync_SendsMessage()
        {
            var mockFactory = new Mock<IServiceBusSenderFactory>();
            var mockSender = new Mock<IServiceBusSender>();
            mockFactory.Setup(f => f.CreateSender(It.IsAny<string>())).Returns(mockSender.Object);

            var config = new Microsoft.Extensions.Configuration.ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string,string>{{"SERVICEBUS_QUEUE_ANALYSIS","test-queue"}}).Build();
            var enqueuer = new ServiceBusEnqueuer(mockFactory.Object, config);

            await enqueuer.EnqueueAnalysisAsync(new { AnalysisId = System.Guid.NewGuid() });

            mockSender.Verify(s => s.SendMessageAsync(It.IsAny<ServiceBusMessage>()), Times.Once);
        }
    }
}
