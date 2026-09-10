using System;
using System.ClientModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpenAI;
using OpenAI.Chat;
using Polly;

namespace SecurityReport.Infrastructure.Services
{
    // Implementacion directa contra la API oficial de OpenAI (SDK oficial "OpenAI" para .NET).
    // Usa la Chat Completions API (estable en esta version del SDK); la Responses API
    // (OpenAI.Responses.ResponsesClient) esta marcada como experimental/evaluacion (OPENAI001)
    // en la version 2.13.0 instalada, por lo que se evita para no depender de una API sujeta a
    // cambios. Nunca registra la API key ni el prompt completo en logs.
    public class OpenAIClientImpl : IAITextClient
    {
        private readonly ChatClient _client;
        private readonly ILogger<OpenAIClientImpl> _logger;
        private readonly string _model;

        public OpenAIClientImpl(IConfiguration config, ILogger<OpenAIClientImpl> logger)
        {
            _logger = logger;
            var apiKey = config["OPENAI_API_KEY"] ?? throw new ArgumentNullException("OPENAI_API_KEY");
            _model = config["OPENAI_MODEL"] ?? throw new ArgumentNullException("OPENAI_MODEL");

            var openAiClient = new OpenAIClient(new ApiKeyCredential(apiKey));
            _client = openAiClient.GetChatClient(_model);
        }

        public async Task<string> GetCompletionAsync(string prompt, int maxTokens = 1000, CancellationToken cancellationToken = default)
        {
            var policy = Policy.Handle<Exception>().WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) });
            var started = DateTime.UtcNow;

            try
            {
                var result = await policy.ExecuteAsync(async () =>
                {
                    var options = new ChatCompletionOptions { MaxOutputTokenCount = maxTokens };
                    var response = await _client.CompleteChatAsync(
                        new[] { new UserChatMessage(prompt) }, options, cancellationToken);
                    return response.Value.Content.Count > 0 ? response.Value.Content[0].Text ?? string.Empty : string.Empty;
                });

                _logger.LogInformation("OpenAI completion succeeded. Provider=OPENAI Model={Model} DurationMs={DurationMs}",
                    _model, (DateTime.UtcNow - started).TotalMilliseconds);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OpenAI completion failed. Provider=OPENAI Model={Model} DurationMs={DurationMs}",
                    _model, (DateTime.UtcNow - started).TotalMilliseconds);
                throw;
            }
        }
    }
}
