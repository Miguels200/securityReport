using System;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;
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

            var handler = new CreateReportHandler(mockRepo.Object);

            var cmd = new CreateReportCommand("T","D", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
            var res = await handler.Handle(cmd, CancellationToken.None);

            Assert.Equal(cmd.Titulo, res.Titulo);
            mockRepo.Verify();
        }

        [Fact]
        public async Task UpdateReportHandler_Should_ReturnNull_When_NotFound()
        {
            var mockRepo = new Mock<IReportRepository>();
            mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Reporte?)null);

            var handler = new UpdateReportHandler(mockRepo.Object);
            var cmd = new UpdateReportCommand(Guid.NewGuid(), "T","D", Guid.NewGuid());

            var res = await handler.Handle(cmd, CancellationToken.None);
            Assert.Null(res);
        }
    }
}
