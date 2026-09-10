using System;
using System.Collections.Generic;

namespace SecurityReport.Application.DTOs
{
    // Proyeccion de solo lectura de AnalisisIA para el endpoint GET /api/analisis/{id}.
    public record AnalisisDto(
        Guid AnalysisId,
        Guid ReporteId,
        string Status,
        string? TipoRiesgo,
        string? NivelRiesgo,
        string? Prioridad,
        string? Justificacion,
        List<string> Recomendaciones,
        string? Origen,
        DateTime? StartedAt,
        DateTime? CompletedAt,
        DateTime CreatedAt,
        // Mensaje generico, NUNCA el detalle tecnico interno (ver AnalisisIA.ErrorMensaje).
        string? MensajeError);
}
