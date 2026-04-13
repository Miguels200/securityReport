using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.DTOs;
using SecurityReport.Application.Interfaces;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Handlers
{
    public class CreateNormativaHandler : IRequestHandler<CreateNormativaCommand, NormativaDto>
    {
        private readonly INormativaRepository _repo;

        public CreateNormativaHandler(INormativaRepository repo)
        {
            _repo = repo;
        }

        public async Task<NormativaDto> Handle(CreateNormativaCommand request, CancellationToken cancellationToken)
        {
            var n = new NormativaSGSST
            {
                Id = System.Guid.NewGuid(),
                Codigo = request.Codigo,
                Titulo = request.Titulo,
                Contenido = request.Contenido
            };

            await _repo.AddAsync(n);
            return new NormativaDto(n.Id, n.Codigo, n.Titulo, n.Contenido);
        }
    }
}