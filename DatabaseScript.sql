-- =====================================================
-- SCRIPT DE CREACIÓN COMPLETA DE BASE DE DATOS
-- Security Report System - SQL Server
-- =====================================================

USE [SecurityReportDB]
GO

-- =====================================================
-- 1. TABLA: Roles
-- =====================================================
CREATE TABLE [dbo].[Roles] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Nombre] NVARCHAR(200) NOT NULL
);
GO

-- Insertar datos iniciales
INSERT INTO [dbo].[Roles] ([Id], [Nombre]) VALUES 
    ('11111111-1111-1111-1111-111111111111', 'Administrador'),
    ('22222222-2222-2222-2222-222222222222', 'ResponsableSST'),
    ('33333333-3333-3333-3333-333333333333', 'Colaborador');
GO

-- =====================================================
-- 2. TABLA: EstadoReporte
-- =====================================================
CREATE TABLE [dbo].[EstadoReporte] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Nombre] NVARCHAR(200) NOT NULL
);
GO

-- Insertar datos iniciales
INSERT INTO [dbo].[EstadoReporte] ([Id], [Nombre]) VALUES 
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Abierto'),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'EnProgreso'),
    ('cccccccc-cccc-cccc-cccc-cccccccccccc', 'Cerrado');
GO

-- =====================================================
-- 3. TABLA: Areas
-- =====================================================
CREATE TABLE [dbo].[Areas] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Nombre] NVARCHAR(200) NOT NULL,
    [Descripcion] NVARCHAR(1000) NULL
);
GO

-- =====================================================
-- 4. TABLA: Usuarios
-- =====================================================
CREATE TABLE [dbo].[Usuarios] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Nombre] NVARCHAR(200) NOT NULL,
    [Email] NVARCHAR(450) NOT NULL,
    [PasswordHash] NVARCHAR(MAX) NOT NULL,
    [RolId] UNIQUEIDENTIFIER NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Usuarios_Roles_RolId] FOREIGN KEY ([RolId]) 
        REFERENCES [dbo].[Roles]([Id]) ON DELETE CASCADE
);
GO

-- Índices para Usuarios
CREATE UNIQUE INDEX [IX_Usuarios_Email] ON [dbo].[Usuarios]([Email]);
CREATE INDEX [IX_Usuarios_RolId] ON [dbo].[Usuarios]([RolId]);
GO

-- =====================================================
-- 5. TABLA: Reportes
-- =====================================================
CREATE TABLE [dbo].[Reportes] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Titulo] NVARCHAR(500) NOT NULL,
    [Descripcion] NVARCHAR(MAX) NOT NULL,
    [AreaId] UNIQUEIDENTIFIER NOT NULL,
    [EstadoReporteId] UNIQUEIDENTIFIER NOT NULL,
    [ReportadoPorId] UNIQUEIDENTIFIER NOT NULL,
    [FechaReporte] DATETIME2 NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Reportes_Areas_AreaId] FOREIGN KEY ([AreaId]) 
        REFERENCES [dbo].[Areas]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Reportes_EstadoReporte_EstadoReporteId] FOREIGN KEY ([EstadoReporteId]) 
        REFERENCES [dbo].[EstadoReporte]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Reportes_Usuarios_ReportadoPorId] FOREIGN KEY ([ReportadoPorId]) 
        REFERENCES [dbo].[Usuarios]([Id]) ON DELETE NO ACTION
);
GO

-- Índices para Reportes
CREATE INDEX [IX_Reportes_FechaReporte] ON [dbo].[Reportes]([FechaReporte]);
CREATE INDEX [IX_Reportes_AreaId] ON [dbo].[Reportes]([AreaId]);
CREATE INDEX [IX_Reportes_EstadoReporteId] ON [dbo].[Reportes]([EstadoReporteId]);
CREATE INDEX [IX_Reportes_ReportadoPorId] ON [dbo].[Reportes]([ReportadoPorId]);
GO

-- =====================================================
-- 6. TABLA: Condiciones (CondicionInsegura)
-- =====================================================
CREATE TABLE [dbo].[Condiciones] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Descripcion] NVARCHAR(MAX) NOT NULL,
    [ReporteId] UNIQUEIDENTIFIER NOT NULL,
    [FechaIdentificacion] DATETIME2 NOT NULL,
    CONSTRAINT [FK_Condiciones_Reportes_ReporteId] FOREIGN KEY ([ReporteId]) 
        REFERENCES [dbo].[Reportes]([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Condiciones_ReporteId] ON [dbo].[Condiciones]([ReporteId]);
GO

-- =====================================================
-- 7. TABLA: Actos (ActoInseguro)
-- =====================================================
CREATE TABLE [dbo].[Actos] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Descripcion] NVARCHAR(MAX) NOT NULL,
    [ReporteId] UNIQUEIDENTIFIER NOT NULL,
    [FechaIdentificacion] DATETIME2 NOT NULL,
    CONSTRAINT [FK_Actos_Reportes_ReporteId] FOREIGN KEY ([ReporteId]) 
        REFERENCES [dbo].[Reportes]([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Actos_ReporteId] ON [dbo].[Actos]([ReporteId]);
GO

-- =====================================================
-- 8. TABLA: Evidencias
-- =====================================================
CREATE TABLE [dbo].[Evidencias] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [FileName] NVARCHAR(500) NOT NULL,
    [ContentType] NVARCHAR(200) NOT NULL,
    [Size] BIGINT NOT NULL,
    [BlobUrl] NVARCHAR(MAX) NOT NULL,
    [ReporteId] UNIQUEIDENTIFIER NOT NULL,
    [UploadedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    CONSTRAINT [FK_Evidencias_Reportes_ReporteId] FOREIGN KEY ([ReporteId]) 
        REFERENCES [dbo].[Reportes]([Id]) ON DELETE CASCADE
);
GO

CREATE INDEX [IX_Evidencias_ReporteId] ON [dbo].[Evidencias]([ReporteId]);
GO

-- =====================================================
-- 9. TABLA: AnalisisIA
-- =====================================================
CREATE TABLE [dbo].[AnalisisIA] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [ReporteId] UNIQUEIDENTIFIER NOT NULL,
    [Tipo] NVARCHAR(200) NOT NULL,
    [ResultadoJson] NVARCHAR(MAX) NOT NULL,
    [Origen] NVARCHAR(200) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [Status] NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    [AttemptCount] INT NOT NULL DEFAULT 0,
    [StartedAt] DATETIME2 NULL,
    [CompletedAt] DATETIME2 NULL,
    CONSTRAINT [FK_AnalisisIA_Reportes_ReporteId] FOREIGN KEY ([ReporteId]) 
        REFERENCES [dbo].[Reportes]([Id]) ON DELETE CASCADE
);
GO

-- Índices para AnalisisIA
CREATE INDEX [IX_AnalisisIA_ReporteId] ON [dbo].[AnalisisIA]([ReporteId]);
CREATE INDEX [IX_AnalisisIA_Status] ON [dbo].[AnalisisIA]([Status]);
GO

-- =====================================================
-- 10. TABLA: InformesIA
-- =====================================================
CREATE TABLE [dbo].[InformesIA] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Titulo] NVARCHAR(500) NOT NULL,
    [Contenido] NVARCHAR(MAX) NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [Periodo] NVARCHAR(200) NOT NULL
);
GO

-- =====================================================
-- 11. TABLA: RiesgosRepetitivos
-- =====================================================
CREATE TABLE [dbo].[RiesgosRepetitivos] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Descripcion] NVARCHAR(MAX) NOT NULL,
    [Ocurrencias] INT NOT NULL,
    [NivelRiesgo] NVARCHAR(50) NOT NULL,
    [FirstDetected] DATETIME2 NOT NULL,
    [LastDetected] DATETIME2 NOT NULL
);
GO

-- =====================================================
-- 12. TABLA: Normativas (NormativaSGSST)
-- =====================================================
CREATE TABLE [dbo].[Normativas] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Codigo] NVARCHAR(200) NOT NULL,
    [Titulo] NVARCHAR(500) NOT NULL,
    [Contenido] NVARCHAR(MAX) NOT NULL
);
GO

-- =====================================================
-- 13. TABLA: LogsAuditoria
-- =====================================================
CREATE TABLE [dbo].[LogsAuditoria] (
    [Id] UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    [Entidad] NVARCHAR(200) NOT NULL,
    [EntidadId] UNIQUEIDENTIFIER NOT NULL,
    [Accion] NVARCHAR(100) NOT NULL,
    [Usuario] NVARCHAR(200) NOT NULL,
    [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [Detalle] NVARCHAR(MAX) NOT NULL
);
GO

-- =====================================================
-- ÍNDICES ADICIONALES PARA PERFORMANCE
-- =====================================================
CREATE INDEX [IX_LogsAuditoria_Timestamp] ON [dbo].[LogsAuditoria]([Timestamp]);
CREATE INDEX [IX_LogsAuditoria_Entidad] ON [dbo].[LogsAuditoria]([Entidad]);
CREATE INDEX [IX_RiesgosRepetitivos_NivelRiesgo] ON [dbo].[RiesgosRepetitivos]([NivelRiesgo]);
GO

-- =====================================================
-- TRIGGERS PARA ACTUALIZACIÓN AUTOMÁTICA
-- =====================================================

-- Trigger para actualizar UpdatedAt en Usuarios
CREATE TRIGGER [dbo].[TR_Usuarios_UpdatedAt]
ON [dbo].[Usuarios]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Usuarios]
    SET [UpdatedAt] = GETUTCDATE()
    FROM [dbo].[Usuarios] u
    INNER JOIN inserted i ON u.Id = i.Id;
END;
GO

-- Trigger para actualizar UpdatedAt en Reportes
CREATE TRIGGER [dbo].[TR_Reportes_UpdatedAt]
ON [dbo].[Reportes]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE [dbo].[Reportes]
    SET [UpdatedAt] = GETUTCDATE()
    FROM [dbo].[Reportes] r
    INNER JOIN inserted i ON r.Id = i.Id;
END;
GO

PRINT 'Base de datos creada exitosamente!';
GO
