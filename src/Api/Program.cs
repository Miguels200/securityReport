using System;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Serilog;
using SecurityReport.Infrastructure.Persistence;
using SecurityReport.Infrastructure.Services;
using SecurityReport.Infrastructure.Background;
using SecurityReport.Api.Middlewares;
using SecurityReport.Infrastructure.Repositories;
using MediatR;
using System.Reflection;
using SecurityReport.Application.Handlers;
using SecurityReport.Application.Interfaces;
using FluentValidation;
using SecurityReport.Application.Commands;
using SecurityReport.Application.Validators;
using Microsoft.ApplicationInsights.Extensibility;
using Azure.Identity;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Integrate Azure Key Vault in non-development environments if configured
var kvUri = builder.Configuration["AZURE_KEYVAULT_URI"];
if (!string.IsNullOrEmpty(kvUri) && !builder.Environment.IsDevelopment())
{
    try
    {
        builder.Configuration.AddAzureKeyVault(new Uri(kvUri), new DefaultAzureCredential());
    }
    catch (Exception ex)
    {
        // If Key Vault is not accessible, fall back to environment variables; log the issue.
        Console.WriteLine($"Warning: unable to connect to Azure Key Vault: {ex.Message}");
    }
}

// Application Insights
var aiKey = builder.Configuration["APPINSIGHTS_INSTRUMENTATIONKEY"];
if (!string.IsNullOrEmpty(aiKey))
{
    builder.Services.AddApplicationInsightsTelemetry(aiKey);
}

// Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Configuration
var configuration = builder.Configuration;

// CORS for web clients (Angular local dev + configurable origins)
var corsOrigins = configuration["CORS_ORIGINS"];
var allowedOrigins = string.IsNullOrWhiteSpace(corsOrigins)
    ? new[] { "http://localhost:4200" }
    : corsOrigins.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Strict validation in non-development environments
if (!builder.Environment.IsDevelopment())
{
    var missing = new System.Collections.Generic.List<string>();
    if (string.IsNullOrWhiteSpace(configuration["DB_CONNECTION"])) missing.Add("DB_CONNECTION");
    if (string.IsNullOrWhiteSpace(configuration["JWT_SECRET"])) missing.Add("JWT_SECRET");
    if (missing.Count > 0)
    {
        throw new InvalidOperationException($"Missing required configuration keys in non-development environment: {string.Join(", ", missing)}");
    }
}

// DB
var connection = configuration["DB_CONNECTION"]; // null if not set
var isUsingInMemory = false;
if (!string.IsNullOrWhiteSpace(connection))
{
    builder.Services.AddDbContext<SecurityReportDbContext>(options => options.UseSqlServer(connection));
}
else if (builder.Environment.IsDevelopment())
{
    // Use InMemory DB for development to allow the app to start without a real DB
    builder.Services.AddDbContext<SecurityReportDbContext>(options => options.UseInMemoryDatabase("dev-db"));
    isUsingInMemory = true;
}
else
{
    // Production: require DB (should have been caught by validation above)
    throw new InvalidOperationException("DB_CONNECTION must be configured in non-development environments.");
}

// Services
builder.Services.AddScoped<IAIAnalysisService, AzureOpenAIService>();
// Azure OpenAI client implementation registration
var azureOpenAiEndpoint = configuration["AZURE_OPENAI_ENDPOINT"];
var azureOpenAiApiKey = configuration["AZURE_OPENAI_API_KEY"];
if (!string.IsNullOrWhiteSpace(azureOpenAiEndpoint) && !string.IsNullOrWhiteSpace(azureOpenAiApiKey))
{
    builder.Services.AddSingleton<IAzureOpenAIClient, AzureOpenAIClientImpl>();
}
else
{
    builder.Services.AddSingleton<IAzureOpenAIClient, NullAzureOpenAIClient>();
}

builder.Services.AddSingleton<IPasswordHasherService, SecurityReport.Infrastructure.Services.PasswordHasherService>();

// Blob Storage - register conditionally
var blobConnection = configuration["BLOB_CONNECTION"];
if (!string.IsNullOrWhiteSpace(blobConnection))
{
    builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
}
else
{
    // Register a no-op blob storage service so app keeps working without Azure Blob Storage
    builder.Services.AddSingleton<IBlobStorageService, NullBlobStorageService>();
}

// Service Bus - register conditionally
var serviceBusConn = configuration["SERVICEBUS_CONNECTION"];
if (!string.IsNullOrWhiteSpace(serviceBusConn))
{
    builder.Services.AddSingleton<IServiceBusClientProvider, ServiceBusClientProvider>();
    builder.Services.AddSingleton<IServiceBusSenderFactory, ServiceBusSenderFactory>();
    builder.Services.AddSingleton<IServiceBusEnqueuer, ServiceBusEnqueuer>();
    builder.Services.AddHostedService<ServiceBusWorker>();
}
else
{
    // Register a no-op enqueuer so app keeps working without Service Bus
    builder.Services.AddSingleton<IServiceBusEnqueuer, NullServiceBusEnqueuer>();
}

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IReportRepository, ReportRepository>();
builder.Services.AddScoped<IAreaRepository, AreaRepository>();
builder.Services.AddScoped<INormativaRepository, NormativaRepository>();
builder.Services.AddScoped<IRiesgoRepository, RiesgoRepository>();
builder.Services.AddScoped<IAnalysisRepository, AnalysisRepository>();

// Message handler for analysis
builder.Services.AddSingleton<AnalysisMessageHandler>();

// MediatR - register services from Application handlers assembly
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(HandlerRegistration).Assembly));

// AutoMapper - register with empty configuration (no profiles needed since we map manually)
builder.Services.AddAutoMapper(cfg => { }, typeof(HandlerRegistration).Assembly);

// FluentValidation for commands - register many validators
builder.Services.AddScoped<IValidator<CreateReportCommand>, CreateReportCommandValidator>();
builder.Services.AddScoped<IValidator<CreateUserCommand>, CreateUserCommandValidator>();
builder.Services.AddScoped<IValidator<CreateAreaCommand>, CreateAreaCommandValidator>();
builder.Services.AddScoped<IValidator<CreateNormativaCommand>, CreateNormativaCommandValidator>();
builder.Services.AddScoped<IValidator<CreateRiesgoCommand>, CreateRiesgoCommandValidator>();
builder.Services.AddScoped<IValidator<TriggerIAAnalysisCommand>, TriggerIAAnalysisCommandValidator>();
builder.Services.AddScoped<IValidator<UpdateReportCommand>, UpdateReportCommandValidator>();
builder.Services.AddScoped<IValidator<DeleteReportCommand>, DeleteReportCommandValidator>();

// Background worker - use ServiceBusWorker in addition to previous worker
// Only register AIAnalysisWorker if DB is configured (or InMemory in dev it's configured above)
builder.Services.AddHostedService<AIAnalysisWorker>();

// Authentication & Authorization
var jwtSecret = configuration["JWT_SECRET"];
if (!string.IsNullOrWhiteSpace(jwtSecret))
{
    builder.Services.AddAuthentication("Bearer").AddJwtBearer("Bearer", options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JWT_ISSUER"],
            ValidAudience = configuration["JWT_AUDIENCE"],
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(configuration["JWT_SECRET"] ?? ""))
        };
    });
}
else if (builder.Environment.IsDevelopment())
{
    // Development fallback: add a simple dev auth handler so local testing works without JWT
    builder.Services.AddAuthentication("Development")
        .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, SecurityReport.Api.DevelopmentAuthHandler>("Development", null);
}
else
{
    // Non-development environment must have JWT configured (should be caught by validation above)
    throw new InvalidOperationException("JWT_SECRET must be configured in non-development environments.");
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("RequireResponsableSST", policy => policy.RequireRole("ResponsableSST", "Administrador"));
});

builder.Services.AddControllers();
// Add FluentValidation integration
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "SecurityReport API", Version = "v1", Description = "Este an�lisis es un apoyo a la toma de decisiones del responsable del SG-SST. La IA no toma decisiones ni ejecuta acciones." });
});

var app = builder.Build();

// Ensure relational DB is created/up-to-date and bootstrap default admin user.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SecurityReportDbContext>();

    if (db.Database.IsRelational())
    {
        db.Database.Migrate();

        var adminRole = db.Roles.FirstOrDefault(r => r.Nombre == "Administrador");
        if (adminRole is null)
        {
            adminRole = new SecurityReport.Domain.Entities.Rol
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Nombre = "Administrador"
            };
            db.Roles.Add(adminRole);
            db.SaveChanges();
        }

        var adminEmail = configuration["DEFAULT_ADMIN_EMAIL"] ?? "admin@empresa.com";
        var adminPassword = configuration["DEFAULT_ADMIN_PASSWORD"] ?? "123";

        var adminUserExists = db.Usuarios.Any(u => u.Email == adminEmail);
        if (!adminUserExists)
        {
            var now = DateTime.UtcNow;
            db.Usuarios.Add(new SecurityReport.Domain.Entities.Usuario
            {
                Id = Guid.NewGuid(),
                Nombre = "Administrador",
                Email = adminEmail,
                PasswordHash = adminPassword,
                RolId = adminRole.Id,
                CreatedAt = now,
                UpdatedAt = now
            });
            db.SaveChanges();
        }
    }
}

// Use auditing middleware
// TEMPORALMENTE DESHABILITADO: causaba FileBufferingReadStream.Disposed
// app.UseMiddleware<AuditingMiddleware>();

// Correlation ID middleware
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("X-Correlation-ID"))
    {
        context.Request.Headers["X-Correlation-ID"] = Guid.NewGuid().ToString();
    }
    context.Response.Headers["X-Correlation-ID"] = context.Request.Headers["X-Correlation-ID"];
    await next();
});

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseRouting();
var uploadsPath = Path.Combine(AppContext.BaseDirectory, "uploads");
Directory.CreateDirectory(uploadsPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads",
    ServeUnknownFileTypes = true
});
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Map health endpoint
app.MapGet("/health", () => Results.Ok(new { status = "Healthy" }));

// Seed in-memory DB with sample data when running in Development without a real DB
if (app.Environment.IsDevelopment() && string.IsNullOrWhiteSpace(configuration["DB_CONNECTION"]))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<SecurityReportDbContext>();

    // Seed only if empty
    if (!db.Roles.Any())
    {
        var rolAdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var rolRespId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var rolColabId = Guid.Parse("33333333-3333-3333-3333-333333333333");

        db.Roles.AddRange(new[] {
            new SecurityReport.Domain.Entities.Rol { Id = rolAdminId, Nombre = "Administrador" },
            new SecurityReport.Domain.Entities.Rol { Id = rolRespId, Nombre = "ResponsableSST" },
            new SecurityReport.Domain.Entities.Rol { Id = rolColabId, Nombre = "Colaborador" }
        });

        var estadoAbierto = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        var estadoEnProgreso = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var estadoCerrado = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc");

        db.EstadosReporte.AddRange(new[] {
            new SecurityReport.Domain.Entities.EstadoReporte { Id = estadoAbierto, Nombre = "Abierto" },
            new SecurityReport.Domain.Entities.EstadoReporte { Id = estadoEnProgreso, Nombre = "EnProgreso" },
            new SecurityReport.Domain.Entities.EstadoReporte { Id = estadoCerrado, Nombre = "Cerrado" }
        });

        var area1Id = Guid.Parse("a1000000-0000-0000-0000-000000000001");
        var area2Id = Guid.Parse("a2000000-0000-0000-0000-000000000002");
        var area3Id = Guid.Parse("a3000000-0000-0000-0000-000000000003");
        var area4Id = Guid.Parse("a4000000-0000-0000-0000-000000000004");
        var area5Id = Guid.Parse("a5000000-0000-0000-0000-000000000005");

        db.Areas.AddRange(new[] {
            new SecurityReport.Domain.Entities.Area { Id = area1Id, Nombre = "Operaciones", Descripcion = "Area de operaciones generales" },
            new SecurityReport.Domain.Entities.Area { Id = area2Id, Nombre = "Planta de Produccion", Descripcion = "Zona de manufactura" },
            new SecurityReport.Domain.Entities.Area { Id = area3Id, Nombre = "Almacen", Descripcion = "Area de almacenamiento" },
            new SecurityReport.Domain.Entities.Area { Id = area4Id, Nombre = "Oficinas", Descripcion = "Area administrativa" },
            new SecurityReport.Domain.Entities.Area { Id = area5Id, Nombre = "Campo", Descripcion = "Trabajo en exterior" }
        });

        var areaId = area1Id;

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
        var userId = Guid.NewGuid();
        db.Usuarios.Add(new SecurityReport.Domain.Entities.Usuario
        {
            Id = userId,
            Nombre = "Administrador Local",
            Email = "admin@empresa.com",
            PasswordHash = "123", // TEMPORAL: Sin hash para pruebas
            // PasswordHash = hasher.Hash("123"),
            RolId = rolAdminId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var reportId = Guid.NewGuid();
        db.Reportes.Add(new SecurityReport.Domain.Entities.Reporte {
            Id = reportId,
            Titulo = "Reporte de prueba",
            Descripcion = "Descripci�n de prueba para Swagger",
            AreaId = areaId,
            EstadoReporteId = estadoAbierto,
            ReportadoPorId = userId,
            FechaReporte = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        db.SaveChanges();
    }
}

// Redirect root to swagger for convenience in dev
if (app.Environment.IsDevelopment())
{
    app.MapGet("/", () => Results.Redirect("/swagger/index.html", true));
}

app.Run();
