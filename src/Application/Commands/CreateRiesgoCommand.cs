using MediatR;
using SecurityReport.Application.DTOs;

namespace SecurityReport.Application.Commands
{
    public record CreateRiesgoCommand(string Descripcion, int Ocurrencias, string NivelRiesgo) : IRequest<RiesgoDto>;
}