using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecurityReport.Infrastructure.Persistence;

namespace SecurityReport.Infrastructure.Repositories
{
    public class RepositoryBase<T> where T : class
    {
        protected readonly SecurityReportDbContext _db;

        public RepositoryBase(SecurityReportDbContext db)
        {
            _db = db;
        }

        public IQueryable<T> Query() => _db.Set<T>();

        public async Task AddAsync(T entity)
        {
            await _db.Set<T>().AddAsync(entity);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(T entity)
        {
            _db.Set<T>().Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(T entity)
        {
            _db.Set<T>().Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}