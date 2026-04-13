using System.Threading.Tasks;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Interfaces
{
    public interface IRiesgoRepository
    {
        Task AddAsync(RiesgoRepetitivo riesgo);
    }
}