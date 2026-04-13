using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.DTOs;
using SecurityReport.Application.Interfaces;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Handlers
{
    public class CreateRiesgoHandler : IRequestHandler<CreateRiesgoCommand, RiesgoDto>
    {
        private readonly IRiesgoRepository _repo;

        public CreateRiesgoHandler(IRiesgoRepository repo)
        {
            _repo = repo;
        }

        public async Task<RiesgoDto> Handle(CreateRiesgoCommand request, CancellationToken cancellationToken)
        {
            var r = new RiesgoRepetitivo
            {
                Id = System.Guid.NewGuid(),
                Descripcion = request.Descripcion,
                Ocurrencias = request.Ocurrencias,
                NivelRiesgo = request.NivelRiesgo,
                FirstDetected = System.DateTime.UtcNow,
                LastDetected = System.DateTime.UtcNow
            };

            await _repo.AddAsync(r);

            return new RiesgoDto(r.Id, r.Descripcion, r.Ocurrencias, r.NivelRiesgo, r.FirstDetected, r.LastDetected);
        }
    }
}