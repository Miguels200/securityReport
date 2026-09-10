using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SecurityReport.Application.DTOs;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Services;

namespace Tests.Unit
{
    public class RiskClassificationServiceTests
    {
        private static RiskClassificationRequest BuildRequest() => new(
            Guid.NewGuid(), "Fuga de gas", "Se detecto fuga de gas en el area de calderas", "Planta",
            "Condicion Insegura", "Zona A", "Fuga de gas", 3, true);

        private static IConfiguration BuildConfig(string? aiProvider = null) =>
            new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                ["AZURE_OPENAI_DEPLOYMENT"] = "gpt-test",
                ["AI_PROVIDER"] = aiProvider
            }).Build();

        [Fact]
        public async Task ClassifyAsync_ValidJson_ReturnsAzureOpenAIOrigin_WhenProviderIsAzure()
        {
            var json = "{\"tipoRiesgo\":\"QUIMICO\",\"nivelRiesgo\":\"ALTO\",\"justificacion\":\"Fuga de gas detectada\",\"recomendaciones\":[\"Evacuar el area\",\"Ventilar\"]}";
            var client = new Mock<IAITextClient>();
            client.Setup(c => c.GetCompletionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(json);

            var service = new AzureOpenAIRiskClassificationService(client.Object, BuildConfig("AZURE_OPENAI"), Mock.Of<ILogger<AzureOpenAIRiskClassificationService>>());
            var result = await service.ClassifyAsync(BuildRequest());

            Assert.Equal(OrigenAnalisis.AZURE_OPENAI, result.Origen);
            Assert.Equal(TipoRiesgo.QUIMICO, result.TipoRiesgo);
            Assert.Equal(NivelRiesgo.ALTO, result.NivelRiesgo);
            Assert.Equal(PrioridadRiesgo.ALTA, result.Prioridad);
            Assert.Equal(2, result.Recomendaciones.Count);
        }

        [Fact]
        public async Task ClassifyAsync_ValidJson_ReturnsOpenAIOrigin_WhenProviderIsOpenAI()
        {
            var json = "{\"tipoRiesgo\":\"FISICO\",\"nivelRiesgo\":\"CRITICO\",\"justificacion\":\"Cable expuesto\",\"recomendaciones\":[\"Desenergizar\"]}";
            var client = new Mock<IAITextClient>();
            client.Setup(c => c.GetCompletionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(json);

            var service = new AzureOpenAIRiskClassificationService(client.Object, BuildConfig("OPENAI"), Mock.Of<ILogger<AzureOpenAIRiskClassificationService>>());
            var result = await service.ClassifyAsync(BuildRequest());

            Assert.Equal(OrigenAnalisis.OPENAI, result.Origen);
            Assert.Equal(TipoRiesgo.FISICO, result.TipoRiesgo);
            Assert.Equal(NivelRiesgo.CRITICO, result.NivelRiesgo);
            Assert.Equal(PrioridadRiesgo.INMEDIATA, result.Prioridad);
        }

        [Fact]
        public async Task ClassifyAsync_InvalidJson_FallsBackToHeuristic()
        {
            var client = new Mock<IAITextClient>();
            client.Setup(c => c.GetCompletionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync("esto no es json valido");

            var service = new AzureOpenAIRiskClassificationService(client.Object, BuildConfig(), Mock.Of<ILogger<AzureOpenAIRiskClassificationService>>());
            var result = await service.ClassifyAsync(BuildRequest());

            Assert.Equal(OrigenAnalisis.HEURISTIC, result.Origen);
            Assert.NotNull(result.TipoRiesgo);
            Assert.NotNull(result.NivelRiesgo);
            Assert.False(string.IsNullOrWhiteSpace(result.Justificacion));
        }

        [Fact]
        public async Task ClassifyAsync_UnknownEnumValue_FallsBackToHeuristic()
        {
            var json = "{\"tipoRiesgo\":\"NO_EXISTE\",\"nivelRiesgo\":\"ALTO\",\"justificacion\":\"x\",\"recomendaciones\":[]}";
            var client = new Mock<IAITextClient>();
            client.Setup(c => c.GetCompletionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(json);

            var service = new AzureOpenAIRiskClassificationService(client.Object, BuildConfig(), Mock.Of<ILogger<AzureOpenAIRiskClassificationService>>());
            var result = await service.ClassifyAsync(BuildRequest());

            Assert.Equal(OrigenAnalisis.HEURISTIC, result.Origen);
        }

        [Fact]
        public async Task ClassifyAsync_EmptyResponse_ReturnsNullClientOrigin()
        {
            var client = new Mock<IAITextClient>();
            client.Setup(c => c.GetCompletionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(string.Empty);

            var service = new AzureOpenAIRiskClassificationService(client.Object, BuildConfig(), Mock.Of<ILogger<AzureOpenAIRiskClassificationService>>());
            var result = await service.ClassifyAsync(BuildRequest());

            Assert.Equal(OrigenAnalisis.NULL_CLIENT, result.Origen);
        }

        [Fact]
        public async Task ClassifyAsync_ClientThrows_ReturnsErrorOriginWithMessage()
        {
            var client = new Mock<IAITextClient>();
            client.Setup(c => c.GetCompletionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Azure OpenAI unavailable"));

            var service = new AzureOpenAIRiskClassificationService(client.Object, BuildConfig(), Mock.Of<ILogger<AzureOpenAIRiskClassificationService>>());
            var result = await service.ClassifyAsync(BuildRequest());

            Assert.Equal(OrigenAnalisis.ERROR, result.Origen);
            Assert.Contains("Azure OpenAI unavailable", result.ErrorMensaje);
            // Continuidad operativa: aunque el origen es ERROR, se completan los campos con heuristica.
            Assert.NotNull(result.NivelRiesgo);
        }
    }
}
