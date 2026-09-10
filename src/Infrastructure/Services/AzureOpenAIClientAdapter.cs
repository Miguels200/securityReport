using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SecurityReport.Infrastructure.Services
{
    // Adapta el IAzureOpenAIClient existente (sin modificarlo) a la abstraccion provider-neutral,
    // reutilizando el deployment configurado y el maxTokens por defecto ya usados en produccion.
    public class AzureOpenAIClientAdapter : IAITextClient
    {
        private readonly IAzureOpenAIClient _client;
        private readonly string _deployment;

        public AzureOpenAIClientAdapter(IAzureOpenAIClient client, IConfiguration config)
        {
            _client = client;
            _deployment = config["AZURE_OPENAI_DEPLOYMENT"] ?? string.Empty;
        }

        public Task<string> GetCompletionAsync(string prompt, int maxTokens = 1000, CancellationToken cancellationToken = default)
            => _client.GetCompletionsAsync(prompt, _deployment, maxTokens: maxTokens);
    }
}
