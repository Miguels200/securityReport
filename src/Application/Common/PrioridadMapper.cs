using System;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Application.Common
{
    // Unica fuente de verdad para la relacion NivelRiesgo -> Prioridad, aprobada en la tesis.
    // La IA determina NivelRiesgo; el backend deriva Prioridad con esta regla fija.
    public static class PrioridadMapper
    {
        public static PrioridadRiesgo DesdeNivel(NivelRiesgo nivel) => nivel switch
        {
            NivelRiesgo.BAJO => PrioridadRiesgo.BAJA,
            NivelRiesgo.MEDIO => PrioridadRiesgo.MEDIA,
            NivelRiesgo.ALTO => PrioridadRiesgo.ALTA,
            NivelRiesgo.CRITICO => PrioridadRiesgo.INMEDIATA,
            _ => throw new ArgumentOutOfRangeException(nameof(nivel), nivel, "Nivel de riesgo no reconocido")
        };
    }
}
