using System.Threading.Tasks;

namespace SecurityReport.Application.Interfaces
{
    public interface IServiceBusEnqueuer
    {
        Task EnqueueAnalysisAsync(object payload);
    }
}
