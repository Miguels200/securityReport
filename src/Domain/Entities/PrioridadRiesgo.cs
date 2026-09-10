namespace SecurityReport.Domain.Entities
{
    // Derivada siempre en backend a partir de NivelRiesgo (ver Application.Common.PrioridadMapper).
    // Nunca debe ser solicitada ni sobrescrita por la respuesta del modelo de IA.
    public enum PrioridadRiesgo
    {
        BAJA,
        MEDIA,
        ALTA,
        INMEDIATA
    }
}
