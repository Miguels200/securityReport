using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityReport.Application.Commands;
using SecurityReport.Application.DTOs;
using SecurityReport.Application.Interfaces;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Handlers
{
    public class CreateAreaHandler : IRequestHandler<CreateAreaCommand, AreaDto>
    {
        private readonly IAreaRepository _repo;

        public CreateAreaHandler(IAreaRepository repo)
        {
            _repo = repo;
        }

        public async Task<AreaDto> Handle(CreateAreaCommand request, CancellationToken cancellationToken)
        {
            var a = new Area
            {
                Id = System.Guid.NewGuid(),
                Nombre = request.Nombre,
                Descripcion = request.Descripcion
            };

            await _repo.AddAsync(a);

            return new AreaDto(a.Id, a.Nombre, a.Descripcion);
        }
    }
}