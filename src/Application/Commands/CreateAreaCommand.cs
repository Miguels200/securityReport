using System;
using MediatR;
using SecurityReport.Application.DTOs;

namespace SecurityReport.Application.Commands
{
    public record CreateAreaCommand(string Nombre, string? Descripcion) : IRequest<AreaDto>;
}