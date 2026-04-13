using System;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Moq;
using Xunit;
using SecurityReport.Infrastructure.Background;
using Microsoft.Extensions.Logging;

namespace Tests.Unit
{
    public class ServiceBusWorkerTests
    {
        [Fact]
        public async Task ProcessMessageHandler_CompletesMessage_OnValidPayload()
        {
            var mockProvider = new Mock<IServiceProvider>();
            var mockClientProvider = new Mock<SecurityReport.Infrastructure.Services.IServiceBusClientProvider>();
            var mockConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
            var logger = new Mock<ILogger<ServiceBusWorker>>();

            var mockHandler = new Mock<AnalysisMessageHandler>(MockBehavior.Strict, mockProvider.Object, mockConfig, new Mock<ILogger<AnalysisMessageHandler>>().Object);
            var worker = new ServiceBusWorker(mockProvider.Object, mockClientProvider.Object, mockConfig, logger.Object, mockHandler.Object);

            var payload = JsonSerializer.Serialize(new { AnalysisId = Guid.NewGuid() });

            var mockArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, null, null, null);
            var msg = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString(payload));
            mockArgs.Setup(a => a.Message).Returns(msg);
            mockArgs.Setup(a => a.CompleteMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), default)).Returns(Task.CompletedTask).Verifiable();

            mockHandler.Setup(h => h.HandleAsync(It.IsAny<Guid>())).Returns(Task.CompletedTask).Verifiable();

            await worker.ProcessMessageHandler(mockArgs.Object);

            mockHandler.Verify();
            mockArgs.Verify();
        }

        [Fact]
        public async Task ProcessMessageHandler_DeadLetters_OnInvalidPayload()
        {
            var mockProvider = new Mock<IServiceProvider>();
            var mockClientProvider = new Mock<SecurityReport.Infrastructure.Services.IServiceBusClientProvider>();
            var mockConfig = new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build();
            var logger = new Mock<ILogger<ServiceBusWorker>>();

            var mockHandler = new Mock<AnalysisMessageHandler>(MockBehavior.Strict, mockProvider.Object, mockConfig, new Mock<ILogger<AnalysisMessageHandler>>().Object);
            var worker = new ServiceBusWorker(mockProvider.Object, mockClientProvider.Object, mockConfig, logger.Object, mockHandler.Object);

            var payload = JsonSerializer.Serialize(new { Invalid = 123 });

            var mockArgs = new Mock<ProcessMessageEventArgs>(MockBehavior.Strict, null, null, null);
            var msg = ServiceBusModelFactory.ServiceBusReceivedMessage(body: BinaryData.FromString(payload));
            mockArgs.Setup(a => a.Message).Returns(msg);
            mockArgs.Setup(a => a.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask).Verifiable();

            await worker.ProcessMessageHandler(mockArgs.Object);

            mockArgs.Verify();
        }
    }
}
