using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml; // needs package
using SecurityReport.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SecurityReport.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ExportController : ControllerBase
    {
        private readonly SecurityReportDbContext _db;

        public ExportController(SecurityReportDbContext db)
        {
            _db = db;
        }

        [HttpGet("reportes/excel")]
        [Authorize(Policy = "RequireResponsableSST")]
        public async Task<IActionResult> ExportReportesExcel()
        {
            var reportes = await _db.Reportes.Include(r => r.Area).Include(r => r.EstadoReporte).ToListAsync();

            using var package = new OfficeOpenXml.ExcelPackage();
            var ws = package.Workbook.Worksheets.Add("Reportes");
            ws.Cells[1,1].Value = "Titulo";
            ws.Cells[1,2].Value = "Descripcion";
            ws.Cells[1,3].Value = "Area";
            ws.Cells[1,4].Value = "Estado";
            ws.Cells[1,5].Value = "FechaReporte";

            var row = 2;
            foreach(var r in reportes)
            {
                ws.Cells[row,1].Value = r.Titulo;
                ws.Cells[row,2].Value = r.Descripcion;
                ws.Cells[row,3].Value = r.Area?.Nombre;
                ws.Cells[row,4].Value = r.EstadoReporte?.Nombre;
                ws.Cells[row,5].Value = r.FechaReporte.ToString("o");
                row++;
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "reportes.xlsx");
        }
    }
}