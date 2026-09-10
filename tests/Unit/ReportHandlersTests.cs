using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
using MediatR;
using Microsoft.Extensions.Logging;
using SecurityReport.Application.Handlers;
using SecurityReport.Application.Commands;
using SecurityReport.Application.Interfaces;
using SecurityReport.Domain.Entities;

namespace Tests.Unit
{
    public class ReportHandlersTests
    {
        [Fact]
        public async Task CreateReportHandler_Should_Add_Report()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Reporte>())).Returns(Task.CompletedTask).Verifiable();
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<TriggerIAAnalysisCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Guid.NewGuid());
            var logger = new Mock<ILogger<CreateReportHandler>>();

            var handler = new CreateReportHandler(mockRepo.Object, mockMediator.Object, logger.Object);

            var cmd = new CreateReportCommand("T","D", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            var res = await handler.Handle(cmd, CancellationToken.None);

            Assert.Equal(cmd.Titulo, res.Titulo);
            mockRepo.Verify();
            mockMediator.Verify(m => m.Send(It.IsAny<TriggerIAAnalysisCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreateReportHandler_Should_Not_Fail_When_Trigger_Throws()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.AddAsync(It.IsAny<Reporte>())).Returns(Task.CompletedTask);
            var mockMediator = new Mock<IMediator>();
            mockMediator.Setup(m => m.Send(It.IsAny<TriggerIAAnalysisCommand>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("service bus down"));
            var logger = new Mock<ILogger<CreateReportHandler>>();

            var handler = new CreateReportHandler(mockRepo.Object, mockMediator.Object, logger.Object);
            var cmd = new CreateReportCommand("T", "D", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

            var res = await handler.Handle(cmd, CancellationToken.None);

            Assert.Equal(cmd.Titulo, res.Titulo);
            Assert.Null(res.AnalysisId);
        }

        [Fact]
        public async Task UpdateReportHandler_Should_ReturnNull_When_NotFound()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Reporte?)null);

            var handler = new UpdateReportHandler(mockRepo.Object);
            var cmd = new UpdateReportCommand(Guid.NewGuid(), "T", "D", "obs", Guid.NewGuid());

            var res = await handler.Handle(cmd, CancellationToken.None);
            Assert.Null(res);
        }
    }
}
