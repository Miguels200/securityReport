using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Domain.Entities;

namespace SecurityReport.Api.Middlewares
{
    public class AuditingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditingMiddleware> _logger;

        public AuditingMiddleware(RequestDelegate next, ILogger<AuditingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Simple auditing for POST/PUT/DELETE on /api/reportes
            if (context.Request.Path.StartsWithSegments("/api/reportes") && (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "DELETE"))
            {
                context.Request.EnableBuffering();
                using var reader = new StreamReader(context.Request.Body);
                var body = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;

                var db = context.RequestServices.GetRequiredService<SecurityReportDbContext>();

                var log = new LogAuditoria
                {
                    Id = Guid.NewGuid(),
                    Entidad = "Reporte",
                    EntidadId = Guid.Empty,
                    Accion = context.Request.Method,
                    Usuario = context.User?.Identity?.Name ?? "anonymous",
                    Timestamp = DateTime.UtcNow,
                    Detalle = body
                };

                db.LogsAuditoria.Add(log);
                await db.SaveChangesAsync();

                _logger.LogInformation("Audit logged for {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            await _next(context);
        }
    }
}