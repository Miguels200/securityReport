using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Threading.Tasks;
using SecurityReport.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportPdfController : ControllerBase
    {
        private readonly SecurityReportDbContext _db;

        public ExportPdfController(SecurityReportDbContext db)
        {
            _db = db;
        }

        [HttpGet("report/{id}")]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> ExportReportPdf(System.Guid id)
        {
            var report = await _db.Reportes.Include(r => r.Area).Include(r => r.EstadoReporte).FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();

            var doc = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Header().Text("Informe de Reporte").Bold().FontSize(14);

                    page.Content().Column(col =>
                    {
                        col.Item().Text($"Titulo: {report.Titulo}");
                        col.Item().Text($"Descripcion: {report.Descripcion}");
                        col.Item().Text($"Area: {report.Area?.Nombre}");
                        col.Item().Text($"Estado: {report.EstadoReporte?.Nombre}");
                        col.Item().Text($"Fecha: {report.FechaReporte:O}");
                    });

                    page.Footer().AlignRight().Text("Security Report - IA analysis support. Este análisis es un apoyo a la toma de decisiones del responsable del SG-SST.");
                });
            });

            var stream = new MemoryStream();
            doc.GeneratePdf(stream);
            stream.Position = 0;
            return File(stream, "application/pdf", $"reporte_{id}.pdf");
        }
    }
}