using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Application.Handlers
{
    public class DeleteReportHandler : IRequestHandler<DeleteReportCommand, bool>
    {
        private readonly IReportRepository _repo;

        public DeleteReportHandler(IReportRepository repo)
        {
            _repo = repo;
        }

        public async Task<bool> Handle(DeleteReportCommand request, CancellationToken cancellationToken)
        {
            var existing = await _repo.GetByIdAsync(request.Id);
            if (existing == null) return false;

            await _repo.DeleteAsync(existing);
            return true;
        }
    }
}