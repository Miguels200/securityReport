using System.Threading.Tasks;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Infrastructure.Repositories
{
    public class NormativaRepository : INormativaRepository
    {
        private readonly SecurityReportDbContext _db;

        public NormativaRepository(SecurityReportDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(NormativaSGSST normativa)
        {
            await _db.Normativas.AddAsync(normativa);
            await _db.SaveChangesAsync();
        }
    }
}