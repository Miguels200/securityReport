using System.Threading.Tasks;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Interfaces
{
    public interface INormativaRepository
    {
        Task AddAsync(NormativaSGSST normativa);
    }
}