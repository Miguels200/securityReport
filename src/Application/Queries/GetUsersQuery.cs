using System.Collections.Generic;
using MediatR;
using SecurityReport.Application.DTOs;

namespace SecurityReport.Application.Queries
{
    public record GetUsersQuery() : IRequest<IEnumerable<UserDto>>;
}