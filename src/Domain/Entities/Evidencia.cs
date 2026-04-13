using System;

namespace SecurityReport.Domain.Entities
{
    public class Evidencia
    {
        public Guid Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public string BlobUrl { get; set; } = string.Empty; // optional external storage
        public Guid ReporteId { get; set; }
        public Reporte? Reporte { get; set; }
        public DateTime UploadedAt { get; set; }
    }
}