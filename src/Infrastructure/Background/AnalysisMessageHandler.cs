using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using SecurityReport.Application.Interfaces;
using SecurityReport.Infrastructure.Services;

namespace SecurityReport.Infrastructure.Background
{
    public class AnalysisMessageHandler
    {
        private readonly IServiceProvider _provider;
        private readonly IConfiguration _config;
        private readonly ILogger<AnalysisMessageHandler> _logger;

        public AnalysisMessageHandler(IServiceProvider provider, IConfiguration config, ILogger<AnalysisMessageHandler> logger)
        {
            _provider = provider;
            _config = config;
            _logger = logger;
        }

        public async Task HandleAsync(Guid analysisId)
        {
            using var scope = _provider.CreateScope();
            var analysisRepo = scope.ServiceProvider.GetRequiredService<IAnalysisRepository>();
            var aiClient = scope.ServiceProvider.GetRequiredService<IAzureOpenAIClient>();
            var db = scope.ServiceProvider.GetRequiredService<SecurityReport.Infrastructure.Persistence.SecurityReportDbContext>();

            var analysis = await analysisRepo.GetByIdAsync(analysisId);
            if (analysis == null)
            {
                _logger.LogWarning("Analysis {Id} not found", analysisId);
                return;
            }

            analysis.Status = "Processing";
            analysis.AttemptCount += 1;
            analysis.StartedAt = DateTime.UtcNow;
            await analysisRepo.UpdateAsync(analysis);

            var report = await db.Reportes.FindAsync(analysis.ReporteId);
            if (report == null)
            {
                analysis.Status = "Failed";
                await analysisRepo.UpdateAsync(analysis);
                return;
            }

            var prompt = $"Analiza y resume: {report.Descripcion}";

            var policy = Polly.Policy.Handle<Exception>().WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) });
            var result = await policy.ExecuteAsync(async () => await aiClient.GetCompletionsAsync(prompt, _config["AZURE_OPENAI_DEPLOYMENT"] ?? string.Empty));

            analysis.ResultadoJson = JsonSerializer.Serialize(new { result, generatedAt = DateTime.UtcNow });
            analysis.Status = "Completed";
            analysis.CompletedAt = DateTime.UtcNow;
            await analysisRepo.UpdateAsync(analysis);

            _logger.LogInformation("Processed analysis {Id}", analysisId);
        }
    }
}
