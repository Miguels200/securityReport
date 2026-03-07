using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Interfaces
{
    public interface IUserRepository
    {
        Task AddAsync(Usuario user);
        Task<Usuario?> GetByIdAsync(Guid id);
        Task<Usuario?> GetByEmailAsync(string email);
        Task<IEnumerable<Usuario>> ListAsync();
    }
}
