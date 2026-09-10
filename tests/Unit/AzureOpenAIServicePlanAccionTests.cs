using System;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using SecurityReport.Domain.Entities;
using SecurityReport.Infrastructure.Services;

namespace Tests.Unit
{
    public class AzureOpenAIServicePlanAccionTests
    {
        private static PlanAccionRequest BuildRequest() => new()
        {
            ReporteId = Guid.NewGuid(),
            Titulo = "Fuga de gas",
            Descripcion = "Se detecto fuga de gas en el area de calderas",
            TipoReporte = "Condicion Insegura",
            NivelRiesgo = "ALTO",
            Area = "Planta",
            Ubicacion = "Zona A",
            Condicion = "Fuga de gas",
            Estado = "Abierto",
            PersonasAfectadas = 3,
            TieneTestigos = true
        };

        private static IConfiguration BuildConfig(string? aiProvider = null) =>
            new ConfigurationBuilder().AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
            {
                ["AZURE_OPENAI_DEPLOYMENT"] = "gpt-test",
                ["AI_PROVIDER"] = aiProvider
            }).Build();

        private static void AssertEstructuraValida(PlanAccionIAResult plan)
        {
            Assert.NotNull(plan.Acciones);
            Assert.NotEmpty(plan.Acciones);
            Assert.NotNull(plan.Recursos);
            Assert.False(string.IsNullOrWhiteSpace(plan.Responsable));
            Assert.NotNull(plan.TiempoEjecucion);
            Assert.NotNull(plan.NormativaAplicable);
            Assert.NotEmpty(plan.NormativaAplicable);
            Assert.False(string.IsNullOrWhiteSpace(plan.Disclaimer));
        }

        [Fact]
        public async System.Threading.Tasks.Task GeneratePlanAccionAsync_ValidJson_OrigenAzureOpenAI_GeneradoConIA()
        {
            var json = @"{
                ""acciones"": [""Retirar el recipiente"", ""Ventilar la zona"", ""Investigar causa raiz""],
                ""recursos"": { ""economicos"": ""$500.000"", ""tiempo"": ""8 horas"", ""personal"": ""2 tecnicos"" },
                ""responsable"": ""Responsable SG-SST"",
                ""tiempoEjecucion"": { ""tipo"": ""INMEDIATO"", ""descripcion"": ""riesgo critico"", ""diasEstimados"": 1 },
                ""normativaAplicable"": [""Resolucion 773 de 2021""],
                ""disclaimer"": ""apoyo a la decision""
            }";

            var client = new Mock<IAITextClient>();
            client.Setup(c => c.GetCompletionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(json);

            var service = new AzureOpenAIService(client.Object, BuildConfig("AZURE_OPENAI"), Mock.Of<ILogger<AzureOpenAIService>>());
            var plan = await service.GeneratePlanAccionAsync(BuildRequest());

            Assert.Equal(OrigenAnalisis.AZURE_OPENAI, plan.Origen);
            Assert.True(plan.GeneradoConIA);
            AssertEstructuraValida(plan);
        }

        [Fact]
        public async System.Threading.Tasks.Task GeneratePlanAccionAsync_ValidJson_OrigenOpenAI_GeneradoConIA()
        {
            var json = @"{
                ""acciones"": [""Desenergizar el circuito"", ""Reemplazar aislamiento"", ""Senalizar la zona""],
                ""recursos"": { ""economicos"": ""$200.000"", ""tiempo"": ""4 horas"", ""personal"": ""1 electricista"" },
                ""responsable"": ""Responsable SG-SST"",
                ""tiempoEjecucion"": { ""tipo"": ""INMEDIATO"", ""descripcion"": ""riesgo electrico"", ""diasEstimados"": 1 },
                ""normativaAplicable"": [""Resolucion 2400 de 1979""],
                ""disclaimer"": ""apoyo a la decision""
            }";

            var client = new Mock<IAITextClient>();
            client.Setup(c => c.GetCompletionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(json);

            var service = new AzureOpenAIService(client.Object, BuildConfig("OPENAI"), Mock.Of<ILogger<AzureOpenAIService>>());
            var plan = await service.GeneratePlanAccionAsync(BuildRequest());

            Assert.Equal(OrigenAnalisis.OPENAI, plan.Origen);
            Assert.True(plan.GeneradoConIA);
            AssertEstructuraValida(plan);
        }

        [Fact]
        public async System.Threading.Tasks.Task GeneratePlanAccionAsync_NonEmptyInvalidJson_OrigenHeuristic_GeneradoConIAFalse()
        {
            var client = new Mock<IAITextClient>();
            client.Setup(c => c.GetCompletionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync("esto no es json valido");

            var service = new AzureOpenAIService(client.Object, BuildConfig(), Mock.Of<ILogger<AzureOpenAIService>>());
            var plan = await service.GeneratePlanAccionAsync(BuildRequest());

            Assert.Equal(OrigenAnalisis.HEURISTIC, plan.Origen);
            Assert.False(plan.GeneradoConIA);
            AssertEstructuraValida(plan);
        }

        [Fact]
        public async System.Threading.Tasks.Task GeneratePlanAccionAsync_EmptyResponse_OrigenNullClient_GeneradoConIAFalse()
        {
            var client = new Mock<IAITextClient>();
            client.Setup(c => c.GetCompletionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(string.Empty);

            var service = new AzureOpenAIService(client.Object, BuildConfig(), Mock.Of<ILogger<AzureOpenAIService>>());
            var plan = await service.GeneratePlanAccionAsync(BuildRequest());

            Assert.Equal(OrigenAnalisis.NULL_CLIENT, plan.Origen);
            Assert.False(plan.GeneradoConIA);
            AssertEstructuraValida(plan);
        }

        [Fact]
        public async System.Threading.Tasks.Task GeneratePlanAccionAsync_ClientThrows_OrigenError_GeneradoConIAFalse_ErrorMensajeRegistrado()
        {
            var client = new Mock<IAITextClient>();
            client.Setup(c => c.GetCompletionAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Azure OpenAI unavailable"));

            var service = new AzureOpenAIService(client.Object, BuildConfig(), Mock.Of<ILogger<AzureOpenAIService>>());
            var plan = await service.GeneratePlanAccionAsync(BuildRequest());

            Assert.Equal(OrigenAnalisis.ERROR, plan.Origen);
            Assert.False(plan.GeneradoConIA);
            Assert.Contains("Azure OpenAI unavailable", plan.ErrorMensaje);
            // Continuidad operativa: el plan heuristico sigue teniendo estructura valida aunque Origen=ERROR.
            AssertEstructuraValida(plan);
        }
    }
}
