using System.Threading.Tasks;
using Xunit;

namespace Tests.Integration
{
    public class WorkerIntegrationTests
    {
        [Fact(Skip = "Requires SQL Server container and env variables")]
        public async Task WorkerProcessesPendingAnalysis()
        {
            // This test is a placeholder. In CI you should start SQL Server container,
            // apply schema, enqueue a pending analysis and assert worker processes it.
            await Task.CompletedTask;
        }
    }
}
