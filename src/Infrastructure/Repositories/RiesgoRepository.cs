using System.Threading.Tasks;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Infrastructure.Repositories
{
    public class RiesgoRepository : IRiesgoRepository
    {
        private readonly SecurityReportDbContext _db;

        public RiesgoRepository(SecurityReportDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(RiesgoRepetitivo riesgo)
        {
            await _db.RiesgosRepetitivos.AddAsync(riesgo);
            await _db.SaveChangesAsync();
        }
    }
}