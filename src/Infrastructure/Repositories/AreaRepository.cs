using System.Threading.Tasks;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Application.Interfaces;

namespace SecurityReport.Infrastructure.Repositories
{
    public class AreaRepository : IAreaRepository
    {
        private readonly SecurityReportDbContext _db;

        public AreaRepository(SecurityReportDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Area area)
        {
            await _db.Areas.AddAsync(area);
            await _db.SaveChangesAsync();
        }
    }
}