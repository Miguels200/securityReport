using System.Threading.Tasks;

namespace SecurityReport.Infrastructure.Services
{
    public interface IAzureOpenAIClient
    {
        Task<string> GetCompletionsAsync(string prompt, string deployment, int maxTokens = 1000);
    }
}