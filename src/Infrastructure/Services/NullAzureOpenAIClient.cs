using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SecurityReport.Infrastructure.Services
{
    public class NullAzureOpenAIClient : IAzureOpenAIClient
    {
        private readonly ILogger<NullAzureOpenAIClient> _logger;

        public NullAzureOpenAIClient(ILogger<NullAzureOpenAIClient> logger)
        {
            _logger = logger;
        }

        public Task<string> GetCompletionsAsync(string prompt, string deployment, int maxTokens = 1000)
        {
            _logger.LogWarning("Azure OpenAI client not configured (AZURE_OPENAI_ENDPOINT/AZURE_OPENAI_API_KEY). Returning empty result for prompt.");
            return Task.FromResult(string.Empty);
        }
    }
}
