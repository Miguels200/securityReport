using System;

namespace SecurityReport.Application.DTOs
{
    // Entrada real disponible del reporte para la clasificacion de riesgo mediante IA.
    // Deliberadamente NO incluye NivelReportadoUsuario para no sesgar la respuesta del modelo.
    public record RiskClassificationRequest(
        Guid ReporteId,
        string Titulo,
        string Descripcion,
        string Area,
        string TipoReporte,
        string Ubicacion,
        string Condicion,
        int PersonasAfectadas,
        bool TieneTestigos);
}
