using System;
using System.Threading.Tasks;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Interfaces
{
    public interface IAreaRepository
    {
        Task AddAsync(Area area);
    }
}