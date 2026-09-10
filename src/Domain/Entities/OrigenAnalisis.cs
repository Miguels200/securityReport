namespace SecurityReport.Domain.Entities
{
    // Indica de donde proviene realmente un resultado de clasificacion/plan de accion.
    // Regla de negocio (obligatoria para las pruebas de la tesis):
    // SOLO los registros con Origen = AZURE_OPENAI pueden considerarse predicciones de IA
    // para calcular Precision/Recall/F1/F1 Macro/matriz de confusion.
    // HEURISTIC, NULL_CLIENT y ERROR NUNCA deben mezclarse con esas metricas.
    public enum OrigenAnalisis
    {
        AZURE_OPENAI,
        OPENAI,
        HEURISTIC,
        NULL_CLIENT,
        ERROR
    }
}
