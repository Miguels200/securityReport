using Microsoft.Extensions.Configuration;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Infrastructure.Services
{
    // Unica fuente de verdad para decidir, a partir de AI_PROVIDER, que OrigenAnalisis corresponde
    // a una clasificacion/plan exitoso. AI_PROVIDER es la fuente principal de seleccion de proveedor.
    public static class AiProviderResolver
    {
        public const string Openai = "OPENAI";
        public const string AzureOpenai = "AZURE_OPENAI";

        // Vacio/no configurado => AZURE_OPENAI, para no romper despliegues existentes sin AI_PROVIDER.
        public static string GetConfiguredProvider(IConfiguration config)
        {
            var value = config["AI_PROVIDER"];
            return string.IsNullOrWhiteSpace(value) ? AzureOpenai : value.Trim().ToUpperInvariant();
        }

        public static OrigenAnalisis GetSuccessOrigin(IConfiguration config) =>
            GetConfiguredProvider(config) == Openai ? OrigenAnalisis.OPENAI : OrigenAnalisis.AZURE_OPENAI;
    }
}
