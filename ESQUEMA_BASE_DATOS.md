# 📊 ESQUEMA COMPLETO DE BASE DE DATOS - SECURITY REPORT SYSTEM

## ✅ RESUMEN DE MIGRACIONES CREADAS

### Migración 1: `20260208_InitialCreate.cs`
Crea la estructura base de todas las tablas.

### Migración 2: `20260209_AddMissingFields.cs` ⭐ NUEVA
Agrega los campos que faltaban en las entidades:
- **Reportes**: `CreatedAt`, `UpdatedAt`
- **AnalisisIA**: `Status`, `AttemptCount`, `StartedAt`, `CompletedAt`
- **Seed Data**: Roles y Estados de Reporte

---

## 📋 LISTADO COMPLETO DE TABLAS (13 TABLAS)

### 1️⃣ **Roles**
Catálogo de roles de usuario en el sistema.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `Nombre` | NVARCHAR(200) | NOT NULL | Nombre del rol |

**Datos Iniciales:**
```sql
'11111111-1111-1111-1111-111111111111' → 'Administrador'
'22222222-2222-2222-2222-222222222222' → 'ResponsableSST'
'33333333-3333-3333-3333-333333333333' → 'Colaborador'
```

---

### 2️⃣ **EstadoReporte**
Catálogo de estados posibles de un reporte.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `Nombre` | NVARCHAR(200) | NOT NULL | Nombre del estado |

**Datos Iniciales:**
```sql
'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa' → 'Abierto'
'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb' → 'EnProgreso'
'cccccccc-cccc-cccc-cccc-cccccccccccc' → 'Cerrado'
```

---

### 3️⃣ **Areas**
Áreas de la empresa donde se reportan incidentes.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `Nombre` | NVARCHAR(200) | NOT NULL | Nombre del área |
| `Descripcion` | NVARCHAR(1000) | NULL | Descripción opcional |

---

### 4️⃣ **Usuarios**
Usuarios del sistema con autenticación.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `Nombre` | NVARCHAR(200) | NOT NULL | Nombre completo |
| `Email` | NVARCHAR(450) | NOT NULL, UNIQUE | Email único |
| `PasswordHash` | NVARCHAR(MAX) | NOT NULL | Contraseña hasheada |
| `RolId` | UNIQUEIDENTIFIER | FK → Roles(Id) CASCADE | Rol asignado |
| `CreatedAt` | DATETIME2 | NOT NULL | Fecha de creación |
| `UpdatedAt` | DATETIME2 | NOT NULL | Última actualización |

**Índices:**
- `IX_Usuarios_Email` (UNIQUE)
- `IX_Usuarios_RolId`

**Relaciones:**
- `Usuarios.RolId` → `Roles.Id` (CASCADE DELETE)

---

### 5️⃣ **Reportes**
Reportes de seguridad y salud ocupacional.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `Titulo` | NVARCHAR(500) | NOT NULL | Título del reporte |
| `Descripcion` | NVARCHAR(MAX) | NOT NULL | Descripción detallada |
| `AreaId` | UNIQUEIDENTIFIER | FK → Areas(Id) CASCADE | Área afectada |
| `EstadoReporteId` | UNIQUEIDENTIFIER | FK → EstadoReporte(Id) CASCADE | Estado actual |
| `ReportadoPorId` | UNIQUEIDENTIFIER | FK → Usuarios(Id) RESTRICT | Usuario reportador |
| `FechaReporte` | DATETIME2 | NOT NULL | Fecha del incidente |
| `CreatedAt` | DATETIME2 | NOT NULL ⭐ | Fecha de creación |
| `UpdatedAt` | DATETIME2 | NOT NULL ⭐ | Última actualización |

**Índices:**
- `IX_Reportes_FechaReporte`
- `IX_Reportes_AreaId`
- `IX_Reportes_EstadoReporteId`
- `IX_Reportes_ReportadoPorId`

**Relaciones:**
- `Reportes.AreaId` → `Areas.Id` (CASCADE DELETE)
- `Reportes.EstadoReporteId` → `EstadoReporte.Id` (CASCADE DELETE)
- `Reportes.ReportadoPorId` → `Usuarios.Id` (RESTRICT DELETE)

---

### 6️⃣ **Condiciones** (CondicionInsegura)
Condiciones inseguras detectadas en un reporte.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `Descripcion` | NVARCHAR(MAX) | NOT NULL | Descripción de la condición |
| `ReporteId` | UNIQUEIDENTIFIER | FK → Reportes(Id) CASCADE | Reporte asociado |
| `FechaIdentificacion` | DATETIME2 | NOT NULL | Fecha de detección |

**Índices:**
- `IX_Condiciones_ReporteId`

**Relaciones:**
- `Condiciones.ReporteId` → `Reportes.Id` (CASCADE DELETE)

---

### 7️⃣ **Actos** (ActoInseguro)
Actos inseguros detectados en un reporte.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `Descripcion` | NVARCHAR(MAX) | NOT NULL | Descripción del acto |
| `ReporteId` | UNIQUEIDENTIFIER | FK → Reportes(Id) CASCADE | Reporte asociado |
| `FechaIdentificacion` | DATETIME2 | NOT NULL | Fecha de detección |

**Índices:**
- `IX_Actos_ReporteId`

**Relaciones:**
- `Actos.ReporteId` → `Reportes.Id` (CASCADE DELETE)

---

### 8️⃣ **Evidencias**
Archivos multimedia asociados a reportes.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `FileName` | NVARCHAR(500) | NOT NULL | Nombre del archivo |
| `ContentType` | NVARCHAR(200) | NOT NULL | Tipo MIME |
| `Size` | BIGINT | NOT NULL | Tamaño en bytes |
| `BlobUrl` | NVARCHAR(MAX) | NOT NULL | URL de Azure Blob |
| `ReporteId` | UNIQUEIDENTIFIER | FK → Reportes(Id) CASCADE | Reporte asociado |
| `UploadedAt` | DATETIME2 | NOT NULL | Fecha de carga |

**Índices:**
- `IX_Evidencias_ReporteId`

**Relaciones:**
- `Evidencias.ReporteId` → `Reportes.Id` (CASCADE DELETE)

---

### 9️⃣ **AnalisisIA**
Análisis de IA realizados sobre los reportes.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `ReporteId` | UNIQUEIDENTIFIER | FK → Reportes(Id) CASCADE | Reporte analizado |
| `Tipo` | NVARCHAR(200) | NOT NULL | Tipo de análisis |
| `ResultadoJson` | NVARCHAR(MAX) | NOT NULL | Resultado en JSON |
| `Origen` | NVARCHAR(200) | NOT NULL | Servicio usado (ej: AzureOpenAI) |
| `CreatedAt` | DATETIME2 | NOT NULL | Fecha de creación |
| `Status` | NVARCHAR(50) | NOT NULL, DEFAULT 'Pending' ⭐ | Estado del procesamiento |
| `AttemptCount` | INT | NOT NULL, DEFAULT 0 ⭐ | Número de intentos |
| `StartedAt` | DATETIME2 | NULL ⭐ | Inicio del procesamiento |
| `CompletedAt` | DATETIME2 | NULL ⭐ | Fin del procesamiento |

**Valores para `Status`:** Pending, Processing, Completed, Failed
**Valores para `Tipo`:** similitud, repetitivo, predictivo, estadistico

**Índices:**
- `IX_AnalisisIA_ReporteId`
- `IX_AnalisisIA_Status` ⭐

**Relaciones:**
- `AnalisisIA.ReporteId` → `Reportes.Id` (CASCADE DELETE)

---

### 🔟 **InformesIA**
Informes generados automáticamente por IA.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `Titulo` | NVARCHAR(500) | NOT NULL | Título del informe |
| `Contenido` | NVARCHAR(MAX) | NOT NULL | Contenido del informe |
| `CreatedAt` | DATETIME2 | NOT NULL | Fecha de generación |
| `Periodo` | NVARCHAR(200) | NOT NULL | Periodo analizado |

---

### 1️⃣1️⃣ **RiesgosRepetitivos**
Riesgos que se repiten frecuentemente.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `Descripcion` | NVARCHAR(MAX) | NOT NULL | Descripción del riesgo |
| `Ocurrencias` | INT | NOT NULL | Número de veces detectado |
| `NivelRiesgo` | NVARCHAR(50) | NOT NULL | Nivel: Bajo, Medio, Alto |
| `FirstDetected` | DATETIME2 | NOT NULL | Primera detección |
| `LastDetected` | DATETIME2 | NOT NULL | Última detección |

**Valores para `NivelRiesgo`:** Bajo, Medio, Alto

---

### 1️⃣2️⃣ **Normativas** (NormativaSGSST)
Normativas de Seguridad y Salud en el Trabajo.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `Codigo` | NVARCHAR(200) | NOT NULL | Código de la normativa |
| `Titulo` | NVARCHAR(500) | NOT NULL | Título de la norma |
| `Contenido` | NVARCHAR(MAX) | NOT NULL | Texto completo |

**Ejemplo:** Código: "Ley 1562", Título: "Sistema de Riesgos Laborales"

---

### 1️⃣3️⃣ **LogsAuditoria**
Registro de auditoría de todas las operaciones.

| Campo | Tipo | Restricciones | Descripción |
|-------|------|---------------|-------------|
| `Id` | UNIQUEIDENTIFIER | PRIMARY KEY | Identificador único |
| `Entidad` | NVARCHAR(200) | NOT NULL | Nombre de la tabla afectada |
| `EntidadId` | UNIQUEIDENTIFIER | NOT NULL | ID del registro afectado |
| `Accion` | NVARCHAR(100) | NOT NULL | Tipo: Create, Update, Delete |
| `Usuario` | NVARCHAR(200) | NOT NULL | Usuario que realizó la acción |
| `Timestamp` | DATETIME2 | NOT NULL | Momento de la acción |
| `Detalle` | NVARCHAR(MAX) | NOT NULL | Información adicional |

**Valores para `Accion`:** Create, Update, Delete

---

## 🎯 COMANDOS PARA APLICAR LAS MIGRACIONES

### Opción 1: Usando Entity Framework Core CLI

```powershell
# Aplicar todas las migraciones
dotnet ef database update --project src\Infrastructure --startup-project src\Api

# Ver el estado de las migraciones
dotnet ef migrations list --project src\Infrastructure --startup-project src\Api

# Generar script SQL de la migración
dotnet ef migrations script --project src\Infrastructure --startup-project src\Api --output migration.sql
```

### Opción 2: Ejecutar el script SQL directamente

```powershell
# El archivo DatabaseScript.sql contiene TODO el esquema completo
# Ejecuta este archivo en SQL Server Management Studio o Azure Data Studio
```

---

## 📊 DIAGRAMA DE RELACIONES

```
Roles (1) ──────────────> (N) Usuarios
                              │
                              │ (1)
                              ▼
Areas (1) ──> (N) Reportes <──┘
                   │
                   │ (1)
EstadoReporte (1) ─┘
                   │
                   ├──> (N) Condiciones
                   ├──> (N) Actos
                   ├──> (N) Evidencias
                   └──> (N) AnalisisIA

[TABLAS INDEPENDIENTES]
- InformesIA
- RiesgosRepetitivos
- Normativas
- LogsAuditoria
```

---

## ✅ VALIDACIONES Y CONSIDERACIONES

### Campos con Default Values:
- `Usuarios.CreatedAt` → GETUTCDATE()
- `Usuarios.UpdatedAt` → GETUTCDATE()
- `Reportes.CreatedAt` → GETUTCDATE()
- `Reportes.UpdatedAt` → GETUTCDATE()
- `AnalisisIA.Status` → 'Pending'
- `AnalisisIA.AttemptCount` → 0
- `Evidencias.UploadedAt` → GETUTCDATE()
- `InformesIA.CreatedAt` → GETUTCDATE()
- `LogsAuditoria.Timestamp` → GETUTCDATE()

### Índices Únicos:
- `Usuarios.Email` (UNIQUE)

### Índices de Performance:
- `Usuarios.RolId`
- `Reportes.FechaReporte`
- `Reportes.AreaId`
- `Reportes.EstadoReporteId`
- `Reportes.ReportadoPorId`
- `Condiciones.ReporteId`
- `Actos.ReporteId`
- `Evidencias.ReporteId`
- `AnalisisIA.ReporteId`
- `AnalisisIA.Status` ⭐ NUEVO

### Relaciones en Cascada:
- Si eliminas un **Rol**, se eliminan sus **Usuarios**
- Si eliminas un **Usuario**, NO se eliminan sus **Reportes** (RESTRICT)
- Si eliminas un **Área**, se eliminan sus **Reportes**
- Si eliminas un **Estado**, se eliminan sus **Reportes**
- Si eliminas un **Reporte**, se eliminan sus **Condiciones, Actos, Evidencias y AnalisisIA**

---

## 🔧 TRIGGERS AUTOMÁTICOS

El script SQL incluye triggers para actualizar automáticamente los campos `UpdatedAt`:

1. **TR_Usuarios_UpdatedAt**: Actualiza `Usuarios.UpdatedAt` en cada UPDATE
2. **TR_Reportes_UpdatedAt**: Actualiza `Reportes.UpdatedAt` en cada UPDATE

---

## 📝 NOTAS IMPORTANTES

⭐ **Campos agregados en la nueva migración `20260209_AddMissingFields.cs`:**
- `Reportes.CreatedAt`
- `Reportes.UpdatedAt`
- `AnalisisIA.Status`
- `AnalisisIA.AttemptCount`
- `AnalisisIA.StartedAt`
- `AnalisisIA.CompletedAt`

✅ **Todas las entidades del dominio ahora están sincronizadas con la base de datos.**

---

## 🚀 SIGUIENTE PASO

Ejecuta uno de estos comandos para crear la base de datos:

```powershell
# Con Entity Framework (Recomendado)
dotnet ef database update --project src\Infrastructure --startup-project src\Api

# O ejecuta el archivo DatabaseScript.sql directamente en SQL Server
```

**¡Todo listo para producción! 🎉**
