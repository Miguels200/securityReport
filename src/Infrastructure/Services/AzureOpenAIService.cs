using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SecurityReport.Infrastructure.Services
{
    public class AzureOpenAIService : IAIAnalysisService
    {
        private readonly IAzureOpenAIClient _client;
        private readonly string _deployment;
        private readonly ILogger<AzureOpenAIService> _logger;

        public AzureOpenAIService(IAzureOpenAIClient client, IConfiguration config, ILogger<AzureOpenAIService> logger)
        {
            _client = client;
            _logger = logger;
            _deployment = config["AZURE_OPENAI_DEPLOYMENT"] ?? throw new ArgumentNullException("AZURE_OPENAI_DEPLOYMENT");
        }

        public async Task<string> AnalyzeReportTextAsync(Guid reporteId, string text, string tipo)
        {
            var prompt = BuildPrompt(text, tipo);
            var result = await _client.GetCompletionsAsync(prompt, _deployment);

            var output = JsonSerializer.Serialize(new
            {
                reporteId,
                tipo,
                analysis = result,
                disclaimer = "Este análisis es un apoyo a la toma de decisiones del responsable del SG-SST. La IA no toma decisiones ni ejecuta acciones."
            });

            _logger.LogInformation("AI analysis completed for {ReporteId} tipo {Tipo}", reporteId, tipo);

            return output;
        }

        private string BuildPrompt(string text, string tipo)
        {
            return $"Analiza el siguiente reporte para detectar {tipo}. Refiere a la normativa SG-SST colombiana y genera hallazgos claros y recomendaciones. Texto: {text}";
        }
    }
}