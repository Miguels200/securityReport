using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SecurityReport.Application.Common;
using SecurityReport.Application.DTOs;
using SecurityReport.Application.Interfaces;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Infrastructure.Services
{
    // Responsabilidad exclusiva: clasificar riesgo mediante IA. Provider-neutral: usa IAITextClient,
    // que puede resolverse a OpenAI directo o a Azure OpenAI segun AI_PROVIDER (ver Program.cs).
    public class AzureOpenAIRiskClassificationService : IRiskClassificationService
    {
        private readonly IAITextClient _client;
        private readonly IConfiguration _config;
        private readonly ILogger<AzureOpenAIRiskClassificationService> _logger;

        public AzureOpenAIRiskClassificationService(IAITextClient client, IConfiguration config, ILogger<AzureOpenAIRiskClassificationService> logger)
        {
            _client = client;
            _config = config;
            _logger = logger;
        }

        public async Task<RiskClassificationResult> ClassifyAsync(RiskClassificationRequest request, CancellationToken cancellationToken = default)
        {
            var prompt = BuildClassificationPrompt(request);
            string rawJson;

            try
            {
                rawJson = await _client.GetCompletionAsync(prompt, maxTokens: 800, cancellationToken);
            }
            catch (Exception ex)
            {
                // Origen = ERROR se conserva incluso si, por continuidad operativa, se completan los
                // campos con el fallback heuristico: el hecho de que el proveedor de IA fallo NUNCA se oculta.
                _logger.LogWarning(ex, "Fallo la llamada al proveedor de IA clasificando el reporte {ReporteId}", request.ReporteId);
                var errorResult = BuildHeuristicClassification(request);
                errorResult.Origen = OrigenAnalisis.ERROR;
                errorResult.ErrorMensaje = Truncate(ex.Message);
                return errorResult;
            }

            if (string.IsNullOrWhiteSpace(rawJson))
            {
                // Cliente no configurado (NullAITextClient) responde vacio sin excepcion.
                var nullClientResult = BuildHeuristicClassification(request);
                nullClientResult.Origen = OrigenAnalisis.NULL_CLIENT;
                return nullClientResult;
            }

            var parsed = TryParseClassification(rawJson);
            if (parsed == null)
            {
                _logger.LogWarning("Respuesta de IA no parseable para el reporte {ReporteId}; se usa fallback heuristico.", request.ReporteId);
                var heuristicResult = BuildHeuristicClassification(request);
                heuristicResult.Origen = OrigenAnalisis.HEURISTIC;
                return heuristicResult;
            }

            parsed.Prioridad = PrioridadMapper.DesdeNivel(parsed.NivelRiesgo!.Value);
            parsed.Origen = AiProviderResolver.GetSuccessOrigin(_config);
            parsed.GeneratedAt = DateTime.UtcNow;
            return parsed;
        }

        // ─── Prompt ────────────────────────────────────────────────────────────────
        // Deliberadamente NO incluye NivelReportadoUsuario: la IA debe clasificar en base a la
        // informacion objetiva del reporte, sin sesgo hacia el valor elegido por el usuario.
        // Deliberadamente NO se solicita "prioridad": la deriva siempre el backend (PrioridadMapper).
        private static string BuildClassificationPrompt(RiskClassificationRequest req)
        {
            return $@"Eres un experto en Seguridad y Salud en el Trabajo (SG-SST) en Colombia.
Clasifica el siguiente reporte de seguridad industrial de forma objetiva, basandote unicamente
en los hechos descritos.

REPORTE:
- Titulo: {req.Titulo}
- Descripcion: {req.Descripcion}
- Area: {req.Area}
- Tipo de reporte: {req.TipoReporte}
- Ubicacion: {req.Ubicacion}
- Condicion/Hallazgo: {req.Condicion}
- Personas afectadas: {req.PersonasAfectadas}
- Tiene testigos: {(req.TieneTestigos ? "SI" : "NO")}

Responde UNICAMENTE con un JSON valido con esta estructura exacta (sin texto adicional):
{{
  ""tipoRiesgo"": ""FISICO|QUIMICO|BIOLOGICO|ERGONOMICO|PSICOSOCIAL"",
  ""nivelRiesgo"": ""BAJO|MEDIO|ALTO|CRITICO"",
  ""justificacion"": ""explicacion breve y objetiva de la clasificacion"",
  ""recomendaciones"": [""recomendacion 1"", ""recomendacion 2""]
}}

Reglas obligatorias:
- tipoRiesgo y nivelRiesgo deben ser EXACTAMENTE uno de los valores permitidos, en mayusculas.
- No incluyas ningun otro campo (por ejemplo prioridad o confianza) fuera de los indicados.
- Las recomendaciones deben ser acciones preventivas o correctivas concretas y accionables.";
        }

        // ─── Parsing tipado (sin JsonDocument/dynamic) ─────────────────────────────
        private RiskClassificationResult? TryParseClassification(string json)
        {
            try
            {
                var start = json.IndexOf('{');
                var end = json.LastIndexOf('}');
                if (start < 0 || end < 0 || end <= start) return null;
                json = json[start..(end + 1)];

                var raw = JsonSerializer.Deserialize<RawClassificationResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (raw == null) return null;

                if (!Enum.TryParse<TipoRiesgo>(raw.TipoRiesgo, ignoreCase: true, out var tipoRiesgo)) return null;
                if (!Enum.TryParse<NivelRiesgo>(raw.NivelRiesgo, ignoreCase: true, out var nivelRiesgo)) return null;

                return new RiskClassificationResult
                {
                    TipoRiesgo = tipoRiesgo,
                    NivelRiesgo = nivelRiesgo,
                    Justificacion = raw.Justificacion ?? string.Empty,
                    Recomendaciones = raw.Recomendaciones?.Where(r => !string.IsNullOrWhiteSpace(r)).ToList() ?? new List<string>()
                };
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "JSON invalido devuelto por Azure OpenAI en clasificacion de riesgo.");
                return null;
            }
        }

        private sealed class RawClassificationResponse
        {
            [JsonPropertyName("tipoRiesgo")]
            public string? TipoRiesgo { get; set; }

            [JsonPropertyName("nivelRiesgo")]
            public string? NivelRiesgo { get; set; }

            [JsonPropertyName("justificacion")]
            public string? Justificacion { get; set; }

            [JsonPropertyName("recomendaciones")]
            public List<string>? Recomendaciones { get; set; }
        }

        // ─── Fallback heuristico (continuidad operativa; nunca cuenta como prediccion de IA) ──
        private static RiskClassificationResult BuildHeuristicClassification(RiskClassificationRequest req)
        {
            var texto = $"{req.Titulo} {req.Descripcion} {req.Condicion} {req.TipoReporte}".ToLowerInvariant();

            var tipoRiesgo = TipoRiesgo.FISICO;
            if (ContieneAlguno(texto, "quimic", "derrame", "acido", "solvent", "gas", "sustancia"))
                tipoRiesgo = TipoRiesgo.QUIMICO;
            else if (ContieneAlguno(texto, "biologic", "virus", "bacteria", "contaminacion biologica", "fluido"))
                tipoRiesgo = TipoRiesgo.BIOLOGICO;
            else if (ContieneAlguno(texto, "ergonom", "postura", "carga", "repetitiv", "lumbar"))
                tipoRiesgo = TipoRiesgo.ERGONOMICO;
            else if (ContieneAlguno(texto, "estres", "acoso", "psicosocial", "carga laboral", "clima laboral"))
                tipoRiesgo = TipoRiesgo.PSICOSOCIAL;

            var nivelRiesgo = NivelRiesgo.MEDIO;
            if (ContieneAlguno(texto, "critico", "explosion", "incendio", "alta tension", "electroc", "asfixia"))
                nivelRiesgo = NivelRiesgo.CRITICO;
            else if (ContieneAlguno(texto, "alto", "caida", "sin arnes", "quimic", "fuga", "atrap", "golpe", "cort"))
                nivelRiesgo = NivelRiesgo.ALTO;
            else if (ContieneAlguno(texto, "bajo", "leve", "menor"))
                nivelRiesgo = NivelRiesgo.BAJO;

            return new RiskClassificationResult
            {
                TipoRiesgo = tipoRiesgo,
                NivelRiesgo = nivelRiesgo,
                Prioridad = PrioridadMapper.DesdeNivel(nivelRiesgo),
                Justificacion = "Clasificacion heuristica generada por reglas de palabras clave (no proviene de Azure OpenAI).",
                Recomendaciones = new List<string>
                {
                    "Revisar manualmente el reporte para confirmar la clasificacion automatica.",
                    "Escalar al responsable SG-SST si el nivel de riesgo aparente es alto o critico."
                },
                GeneratedAt = DateTime.UtcNow
            };
        }

        private static bool ContieneAlguno(string texto, params string[] palabras)
            => palabras.Any(p => texto.Contains(p, StringComparison.OrdinalIgnoreCase));

        private static string Truncate(string? value, int maxLength = 300)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value[..maxLength];
        }
    }
}
