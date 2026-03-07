using MediatR;
using SecurityReport.Application.DTOs;

namespace SecurityReport.Application.Commands
{
    public record CreateNormativaCommand(string Codigo, string Titulo, string Contenido) : IRequest<NormativaDto>;
}