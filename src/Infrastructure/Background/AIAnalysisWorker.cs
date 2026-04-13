using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using SecurityReport.Infrastructure.Persistence;
using System.Linq;
using SecurityReport.Application.Interfaces;
using System.Text.Json;

namespace SecurityReport.Infrastructure.Background
{
    public class AIAnalysisWorker : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<AIAnalysisWorker> _logger;

        public AIAnalysisWorker(IServiceProvider provider, ILogger<AIAnalysisWorker> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AI Analysis Worker started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _provider.CreateScope();
                    var analysisRepo = scope.ServiceProvider.GetRequiredService<IAnalysisRepository>();
                    var aiService = scope.ServiceProvider.GetRequiredService<SecurityReport.Infrastructure.Services.IAzureOpenAIClient>();
                    var db = scope.ServiceProvider.GetRequiredService<SecurityReportDbContext>();

                    var pending = await analysisRepo.GetPendingAsync();
                    if (pending == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                        continue;
                    }

                    pending.Status = "Processing";
                    pending.StartedAt = DateTime.UtcNow;
                    pending.AttemptCount += 1;
                    await analysisRepo.UpdateAsync(pending);

                    var report = await db.Reportes.FindAsync(pending.ReporteId);
                    if (report == null)
                    {
                        pending.Status = "Failed";
                        await analysisRepo.UpdateAsync(pending);
                        continue;
                    }

                    var prompt = $"Analiza y resume: {report.Descripcion}";
                    var deployment = scope.ServiceProvider.GetRequiredService<Microsoft.Extensions.Configuration.IConfiguration>()["AZURE_OPENAI_DEPLOYMENT"] ?? string.Empty;
                    var result = await aiService.GetCompletionsAsync(prompt, deployment);

                    pending.ResultadoJson = JsonSerializer.Serialize(new { result, generatedAt = DateTime.UtcNow });
                    pending.Status = "Completed";
                    pending.CompletedAt = DateTime.UtcNow;
                    await analysisRepo.UpdateAsync(pending);

                    _logger.LogInformation("Processed analysis {Id}", pending.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AIAnalysisWorker");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}