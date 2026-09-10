using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace SecurityReport.Infrastructure.Services
{
    // Cliente neutro cuando ningun proveedor de IA esta configurado. Responde vacio sin excepcion,
    // igual que NullAzureOpenAIClient, para que el sistema siga operando con fallback heuristico.
    public class NullAITextClient : IAITextClient
    {
        private readonly ILogger<NullAITextClient> _logger;

        public NullAITextClient(ILogger<NullAITextClient> logger)
        {
            _logger = logger;
        }

        public Task<string> GetCompletionAsync(string prompt, int maxTokens = 1000, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Ningun proveedor de IA esta configurado (AI_PROVIDER/credenciales). Devolviendo resultado vacio.");
            return Task.FromResult(string.Empty);
        }
    }
}
