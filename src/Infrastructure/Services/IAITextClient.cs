using System.Threading;
using System.Threading.Tasks;

namespace SecurityReport.Infrastructure.Services
{
    // Abstraccion minima, provider-neutral, para obtener una completion de texto de un LLM.
    // Permite soportar OpenAI directo y Azure OpenAI sin acoplar la logica de clasificacion/plan
    // de accion a un proveedor concreto.
    public interface IAITextClient
    {
        Task<string> GetCompletionAsync(string prompt, int maxTokens = 1000, CancellationToken cancellationToken = default);
    }
}
