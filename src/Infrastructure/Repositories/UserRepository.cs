using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Infrastructure.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly SecurityReportDbContext _db;

        public UserRepository(SecurityReportDbContext db)
        {
            _db = db;
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task AddAsync(Usuario user)
        {
            await _db.Usuarios.AddAsync(user);
            await _db.SaveChangesAsync();
        }

        public async Task<Usuario?> GetByIdAsync(Guid id)
        {
            return await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<Usuario>> ListAsync()
        {
            return await _db.Usuarios.Include(u => u.Rol).ToListAsync();
        }
    }
}