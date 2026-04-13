using System;
using System.Collections.Generic;
using System.Linq;
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
                disclaimer = "Este analisis es un apoyo a la toma de decisiones del responsable del SG-SST. La IA no toma decisiones ni ejecuta acciones."
            });

            _logger.LogInformation("AI analysis completed for {ReporteId} tipo {Tipo}", reporteId, tipo);
            return output;
        }

        public async Task<PlanAccionIAResult> GeneratePlanAccionAsync(PlanAccionRequest req)
        {
            var normativa = SeleccionarNormativa(req);
            var prompt = BuildPlanAccionPrompt(req, normativa);

            string rawJson = string.Empty;
            bool generadoConIA = false;

            try
            {
                rawJson = await _client.GetCompletionsAsync(prompt, _deployment, maxTokens: 2000);
                generadoConIA = !string.IsNullOrWhiteSpace(rawJson);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "IA no disponible para plan de accion de reporte {Id}, usando reglas heuristicas.", req.ReporteId);
            }

            if (generadoConIA && !string.IsNullOrWhiteSpace(rawJson))
            {
                var parsed = TryParseIA(rawJson, normativa, req);
                if (parsed != null)
                {
                    parsed.GeneradoConIA = true;
                    return parsed;
                }
            }

            // Fallback heuristico basado en nivel de riesgo y tipo
            return BuildHeuristicPlan(req, normativa);
        }

        // ─── Prompt ────────────────────────────────────────────────────────────────

        private static string BuildPlanAccionPrompt(PlanAccionRequest req, List<string> normativa)
        {
            var normativaStr = string.Join(", ", normativa);
            return $@"Eres un experto en Seguridad y Salud en el Trabajo (SG-SST) en Colombia. 
Analiza el siguiente reporte de seguridad industrial y genera un plan de accion estructurado.

REPORTE:
- Titulo: {req.Titulo}
- Descripcion: {req.Descripcion}
- Tipo: {req.TipoReporte}
- Nivel de Riesgo: {req.NivelRiesgo}
- Area: {req.Area}
- Ubicacion: {req.Ubicacion}
- Condicion/Hallazgo: {req.Condicion}
- Personas afectadas: {req.PersonasAfectadas}
- Tiene testigos: {(req.TieneTestigos ? "SI" : "NO")}

NORMATIVA APLICABLE: {normativaStr}

Responde UNICAMENTE con un JSON valido con esta estructura exacta (sin texto adicional):
{{
  ""acciones"": [""accion 1"", ""accion 2"", ""accion 3""],
  ""recursos"": {{
    ""economicos"": ""descripcion de recursos economicos estimados"",
    ""tiempo"": ""horas o dias de trabajo del personal requerido"",
    ""personal"": ""perfil y cantidad de personas necesarias""
  }},
  ""responsable"": ""cargo o rol responsable de liderar el plan"",
  ""tiempoEjecucion"": {{
    ""tipo"": ""INMEDIATO"",
    ""descripcion"": ""justificacion del plazo"",
    ""diasEstimados"": 1
  }},
  ""normativaAplicable"": [{string.Join(", ", normativa.Select(n => $@"""{n}"""))}],
  ""disclaimer"": ""Este plan es un apoyo a la toma de decisiones. El responsable SG-SST debe validarlo.""
}}

Reglas obligatorias:
- Genera acciones especificas siguiendo la jerarquia de control: eliminacion, sustitucion, controles de ingenieria, controles administrativos y EPP.
- Si una capa no aplica, omitela.
- Prioriza eliminar o aislar el peligro antes de proponer capacitacion o EPP.
- Ajusta tiempoEjecucion.diasEstimados y recursos.tiempo segun el nivel de riesgo, cantidad de personas afectadas y si hay testigos.
- Si hay testigos o mas de 10 personas afectadas, incrementa tiempo para investigacion, entrevistas y cierre documental.

Para tiempoEjecucion.tipo usa solo: INMEDIATO (0-3 dias), CORTO_PLAZO (4-30 dias), MEDIANO_PLAZO (1-6 meses).";
        }

        private static string BuildPrompt(string text, string tipo)
        {
            return $"Analiza el siguiente reporte para detectar {tipo}. Refiere a la normativa SG-SST colombiana y genera hallazgos claros y recomendaciones. Texto: {text}";
        }

        // ─── Seleccion de normativa ─────────────────────────────────────────────────

        private static List<string> SeleccionarNormativa(PlanAccionRequest req)
        {
            var normativa = new List<string> { "GTC-45 (Identificacion de peligros)", "Decreto 1072 de 2015 (SG-SST)", "Resolucion 0312 de 2019 (Estandares minimos SG-SST)" };
            var texto = $"{req.Titulo} {req.Descripcion} {req.Condicion} {req.TipoReporte}".ToLowerInvariant();

            if (ContieneAlguno(texto, "altura", "elevad", "techo", "andamio", "escalera", "eslingas", "arnes"))
                normativa.Add("Resolucion 4272 de 2021 (Trabajo en alturas)");

            if (ContieneAlguno(texto, "confinado", "silo", "tanque", "pozo", "ducto", "camara"))
                normativa.Add("Resolucion 0491 de 2020 (Espacios confinados)");

            if (ContieneAlguno(texto, "quimico", "quimica", "sustancia", "producto", "derrame", "acido", "solvent", "gas"))
                normativa.Add("Resolucion 773 de 2021 (Sustancias quimicas)");

            if (ContieneAlguno(texto, "electrico", "corriente", "tension", "voltaje", "cables", "tablero"))
                normativa.Add("Resolucion 2400 de 1979 (Higiene y seguridad industrial)");

            if (ContieneAlguno(texto, "incendio", "fuego", "extintor", "combustibl", "explosion"))
                normativa.Add("Resolucion 2400 de 1979 Art. Prevencion incendios)");

            if (ContieneAlguno(texto, "maquinaria", "equipo", "atrapamiento", "proteccion", "guarda"))
                normativa.Add("Resolucion 2400 de 1979 (Maquinas y equipos)");

            if (ContieneAlguno(texto, "incidente", "accidente", "lesion", "herido"))
                normativa.Add("Resolucion 1401 de 2007 (Investigacion de incidentes)");

            if (ContieneAlguno(texto, "ergonomia", "postura", "carga", "repetitiv", "lumbar"))
                normativa.Add("GTC-45 Riesgos biomecánicos");

            if (ContieneAlguno(texto, "ruido", "vibracion", "iluminacion", "temperatura", "ventilacion"))
                normativa.Add("Resolucion 2400 de 1979 (Higiene industrial)");

            if (ContieneAlguno(texto, "condicion insegura", "condicion"))
                normativa.Add("Resolucion 2400 de 1979 (Condiciones de seguridad)");

            normativa.Add("Ley 1562 de 2012 (Sistema General de Riesgos Laborales)");
            return normativa.Distinct().ToList();
        }

        private static bool ContieneAlguno(string texto, params string[] palabras)
            => palabras.Any(p => texto.Contains(p, StringComparison.OrdinalIgnoreCase));

        // ─── Parsing ─────────────────────────────────────────────────────────────────

        private PlanAccionIAResult? TryParseIA(string json, List<string> normativa, PlanAccionRequest req)
        {
            try
            {
                var start = json.IndexOf('{');
                var end = json.LastIndexOf('}');
                if (start < 0 || end < 0) return null;
                json = json[start..(end + 1)];

                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                var acciones = root.TryGetProperty("acciones", out var ac)
                    ? ac.EnumerateArray().Select(x => x.GetString() ?? "").Where(s => !string.IsNullOrWhiteSpace(s)).ToList()
                    : new List<string>();

                RecursosIA recursos = new();
                if (root.TryGetProperty("recursos", out var res))
                {
                    recursos.Economicos = res.TryGetProperty("economicos", out var e) ? e.GetString() ?? "" : "";
                    recursos.Tiempo     = res.TryGetProperty("tiempo", out var t)     ? t.GetString() ?? "" : "";
                    recursos.Personal   = res.TryGetProperty("personal", out var p)   ? p.GetString() ?? "" : "";
                }

                var responsable = root.TryGetProperty("responsable", out var rp) ? rp.GetString() ?? "" : "";

                TiempoEjecucionIA tiempo = new();
                if (root.TryGetProperty("tiempoEjecucion", out var te))
                {
                    tiempo.Tipo           = te.TryGetProperty("tipo", out var tt)          ? tt.GetString() ?? "CORTO_PLAZO" : "CORTO_PLAZO";
                    tiempo.Descripcion    = te.TryGetProperty("descripcion", out var td)   ? td.GetString() ?? "" : "";
                    tiempo.DiasEstimados  = te.TryGetProperty("diasEstimados", out var tde) ? tde.GetInt32() : 7;
                }

                var disclaimer = root.TryGetProperty("disclaimer", out var dis) ? dis.GetString() ?? "" : "";

                return new PlanAccionIAResult
                {
                    Acciones = acciones,
                    Recursos = recursos,
                    Responsable = responsable,
                    TiempoEjecucion = tiempo,
                    NormativaAplicable = normativa,
                    Disclaimer = disclaimer
                };
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "No se pudo parsear JSON de IA para plan de accion.");
                return null;
            }
        }

        // ─── Fallback heuristico ──────────────────────────────────────────────────────

        private static PlanAccionIAResult BuildHeuristicPlan(PlanAccionRequest req, List<string> normativa)
        {
            var riesgo = req.NivelRiesgo.ToUpperInvariant();
            var tipo = req.TipoReporte.ToLowerInvariant();
            bool esCriticoOAlto = riesgo == "CRÍTICO" || riesgo == "CRITICO" || riesgo == "ALTO";

            var personas = req.PersonasAfectadas <= 0 ? 1 : req.PersonasAfectadas;
            var acciones = new List<string>();

            if (ContieneAlguno(tipo + " " + req.Condicion, "quimic", "derrame", "pintura", "solvent", "acido", "gas"))
            {
                acciones.Add($"Eliminar el recipiente o sustancia expuesta de la zona {req.Ubicacion} y trasladarla a almacenamiento seguro con contencion secundaria certificada.");
                acciones.Add("Sustituir el recipiente deteriorado por uno homologado, rotulado bajo SGA y compatible con la sustancia almacenada.");
                acciones.Add("Instalar bandeja de contencion, gabinete o sistema de almacenamiento seguro con ventilacion y segregacion por compatibilidad quimica.");
            }
            else if (ContieneAlguno(tipo + " " + req.Condicion, "altura", "andamio", "escalera", "arnes"))
            {
                acciones.Add("Suspender de inmediato la tarea en altura hasta eliminar la condicion insegura detectada.");
                acciones.Add("Sustituir el equipo, anclaje o elemento defectuoso por uno certificado y apto para trabajo en alturas.");
                acciones.Add("Implementar linea de vida, puntos de anclaje certificados y barreras fisicas para control de caidas.");
            }
            else if (ContieneAlguno(tipo + " " + req.Condicion, "maquina", "maquinaria", "equipo", "atrap"))
            {
                acciones.Add($"Retirar de servicio el equipo involucrado en {req.Area} hasta corregir la condicion insegura.");
                acciones.Add("Sustituir guardas, sensores o componentes defectuosos por dispositivos certificados.");
                acciones.Add("Instalar guardas fisicas, enclavamientos y bloqueo-etiquetado para impedir exposicion al punto de operacion.");
            }
            else
            {
                acciones.Add($"Eliminar la condicion insegura identificada en {req.Ubicacion} mediante retiro, orden o correccion fisica del hallazgo.");
                acciones.Add("Sustituir el elemento, metodo o practica insegura por una alternativa de menor riesgo.");
                acciones.Add("Implementar controles de ingenieria que aíslen o reduzcan la exposicion al peligro detectado.");
            }

            acciones.Add($"Establecer controles administrativos: inspecciones, señalizacion, permiso de trabajo y reinduccion para las {personas} persona(s) potencialmente afectadas.");
            acciones.Add("Definir y exigir el EPP aplicable mientras los controles superiores quedan implementados y verificados.");
            acciones.Add($"Investigar la causa raiz del hallazgo, entrevistar {(req.TieneTestigos ? "testigos y personal involucrado" : "personal involucrado")} y cerrar el informe tecnico con trazabilidad de acciones.");
            acciones.Add("Verificar eficacia del control implementado con inspeccion final, registro fotografico y aprobacion del responsable SG-SST.");

            if (tipo.Contains("incidente") || tipo.Contains("accidente"))
                acciones.Insert(2, "Notificar a la ARL y realizar investigacion del incidente segun Res. 1401/2007");

            string tipoTiempo;
            string descTiempo;
            int diasBase;

            if (esCriticoOAlto)
            {
                tipoTiempo = "INMEDIATO";
                descTiempo = $"Nivel de riesgo {req.NivelRiesgo} requiere accion inmediata para evitar lesiones graves o fatalidades.";
                diasBase = riesgo == "CRÍTICO" || riesgo == "CRITICO" ? 1 : 3;
            }
            else if (riesgo == "MEDIO")
            {
                tipoTiempo = "CORTO_PLAZO";
                descTiempo = "Riesgo medio: implementar controles en un plazo maximo de 15 dias habiles.";
                diasBase = 15;
            }
            else
            {
                tipoTiempo = "MEDIANO_PLAZO";
                descTiempo = "Riesgo bajo: planificar mejoras en el siguiente ciclo de revision del SG-SST (maximo 60 dias).";
                diasBase = 60;
            }

            var dias = diasBase;
            if (personas >= 5) dias += 2;
            if (personas > 10) dias += 4;
            if (req.TieneTestigos) dias += 1;

            if (tipoTiempo == "INMEDIATO")
            {
                dias = Math.Min(dias, 3);
            }
            else if (tipoTiempo == "CORTO_PLAZO")
            {
                dias = Math.Min(dias, 30);
            }
            else
            {
                dias = Math.Min(dias, 180);
            }

            var horasHombre = Math.Max(8, (personas * 6) + (req.TieneTestigos ? 6 : 0) + (esCriticoOAlto ? 24 : 8));
            descTiempo += $" Se consideraron {personas} persona(s) afectadas{(req.TieneTestigos ? " y toma de testimonios" : string.Empty)} para estimar el cierre.";

            return new PlanAccionIAResult
            {
                Acciones = acciones,
                Recursos = new RecursosIA
                {
                    Economicos = esCriticoOAlto
                        ? "Recursos de emergencia del presupuesto SG-SST para eliminacion del peligro, adecuaciones fisicas, señalizacion, reposicion de equipos y cierre documental."
                        : "Presupuesto operativo del SG-SST para sustitucion, controles de ingenieria, señalizacion y reinduccion del personal.",
                    Tiempo = $"Estimado {horasHombre} horas-hombre para investigacion, implementacion de controles y verificacion de cierre.",
                    Personal = $"Responsable SG-SST, supervisor del area, mantenimiento/infraestructura segun aplique y atencion sobre {personas} persona(s) potencialmente afectadas{(req.TieneTestigos ? ", incluyendo entrevistas a testigos" : string.Empty)}."
                },
                Responsable = "Responsable del Sistema de Gestion de Seguridad y Salud en el Trabajo (SG-SST)",
                TiempoEjecucion = new TiempoEjecucionIA
                {
                    Tipo = tipoTiempo,
                    Descripcion = descTiempo,
                    DiasEstimados = dias
                },
                NormativaAplicable = normativa,
                Disclaimer = "Plan generado automaticamente como apoyo al responsable del SG-SST. Debe ser validado y ajustado segun el contexto especifico.",
                GeneradoConIA = false
            };
        }
    }
}
