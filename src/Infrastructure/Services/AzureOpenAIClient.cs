using System;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Polly;

namespace SecurityReport.Infrastructure.Services
{
    public class AzureOpenAIClientImpl : IAzureOpenAIClient
    {
        private readonly OpenAIClient _client;
        private readonly ILogger<AzureOpenAIClientImpl> _logger;

        public AzureOpenAIClientImpl(IConfiguration config, ILogger<AzureOpenAIClientImpl> logger)
        {
            _logger = logger;
            var endpoint = config["AZURE_OPENAI_ENDPOINT"] ?? throw new ArgumentNullException("AZURE_OPENAI_ENDPOINT");
            var apiKey = config["AZURE_OPENAI_API_KEY"] ?? throw new ArgumentNullException("AZURE_OPENAI_API_KEY");

            var cred = new AzureKeyCredential(apiKey);
            _client = new OpenAIClient(new Uri(endpoint), cred);
        }

        public async Task<string> GetCompletionsAsync(string prompt, string deployment, int maxTokens = 1000)
        {
            var policy = Policy.Handle<Exception>().WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(4) });

            return await policy.ExecuteAsync(async () =>
            {
                var response = await _client.GetCompletionsAsync(deployment, new CompletionsOptions { MaxTokens = maxTokens, Prompts = { prompt } });
                return response.Value.Choices[0].Text ?? string.Empty;
            });
        }
    }
}