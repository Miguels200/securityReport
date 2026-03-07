using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using SecurityReport.Application.Queries;
using SecurityReport.Application.DTOs;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Application.Handlers
{
    public class GetUsersHandler : IRequestHandler<GetUsersQuery, IEnumerable<UserDto>>
    {
        private readonly IUserRepository _repo;

        public GetUsersHandler(IUserRepository repo)
        {
            _repo = repo;
        }

        public async Task<IEnumerable<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        {
            // For simplicity, repository doesn't implement listing; use DB access in repo if needed. Placeholder returns empty.
            return Enumerable.Empty<UserDto>();
        }
    }
}