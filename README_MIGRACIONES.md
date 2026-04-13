# 🎉 MIGRACIONES COMPLETADAS - SECURITY REPORT SYSTEM

## ✅ ARCHIVOS CREADOS

### 1. Migraciones
- ✅ `src\Infrastructure\Migrations\20260209_AddMissingFields.cs` - Migración con campos faltantes
- ✅ `src\Infrastructure\Migrations\AddMissingFields.Designer.cs` - Designer de migración

### 2. Scripts SQL
- ✅ `DatabaseScript.sql` - Script completo para crear toda la base de datos manualmente

### 3. Documentación
- ✅ `ESQUEMA_BASE_DATOS.md` - Documentación completa del esquema (13 tablas)

### 4. Scripts de Automatización
- ✅ `Apply-Migrations.ps1` - Script PowerShell para aplicar migraciones fácilmente

---

## 📊 RESUMEN DE TABLAS (13 TABLAS)

| # | Tabla | Registros | Descripción |
|---|-------|-----------|-------------|
| 1 | **Roles** | 3 seed | Administrador, ResponsableSST, Colaborador |
| 2 | **EstadoReporte** | 3 seed | Abierto, EnProgreso, Cerrado |
| 3 | **Areas** | 0 | Áreas de la empresa |
| 4 | **Usuarios** | 0 | Usuarios del sistema |
| 5 | **Reportes** | 0 | Reportes de seguridad ⭐ ACTUALIZADO |
| 6 | **Condiciones** | 0 | Condiciones inseguras |
| 7 | **Actos** | 0 | Actos inseguros |
| 8 | **Evidencias** | 0 | Archivos multimedia |
| 9 | **AnalisisIA** | 0 | Análisis de IA ⭐ ACTUALIZADO |
| 10 | **InformesIA** | 0 | Informes generados por IA |
| 11 | **RiesgosRepetitivos** | 0 | Riesgos recurrentes |
| 12 | **Normativas** | 0 | Normativas SGSST |
| 13 | **LogsAuditoria** | 0 | Auditoría del sistema |

---

## ⭐ CAMPOS AGREGADOS EN LA NUEVA MIGRACIÓN

### Tabla: **Reportes**
```sql
CreatedAt    DATETIME2    NOT NULL    DEFAULT GETUTCDATE()
UpdatedAt    DATETIME2    NOT NULL    DEFAULT GETUTCDATE()
```

### Tabla: **AnalisisIA**
```sql
Status         NVARCHAR(50)    NOT NULL    DEFAULT 'Pending'
AttemptCount   INT             NOT NULL    DEFAULT 0
StartedAt      DATETIME2       NULL
CompletedAt    DATETIME2       NULL
```

**Nuevo Índice:** `IX_AnalisisIA_Status`

---

## 🚀 CÓMO APLICAR LAS MIGRACIONES

### Opción 1: Script PowerShell (Recomendado)

```powershell
.\Apply-Migrations.ps1
```

El script te guiará paso a paso y te dará 3 opciones:
1. ✅ Aplicar migraciones directamente
2. 📄 Generar script SQL
3. ❌ Cancelar

### Opción 2: Comandos Manuales

```powershell
# Ver lista de migraciones
dotnet ef migrations list --project src\Infrastructure --startup-project src\Api

# Aplicar todas las migraciones
dotnet ef database update --project src\Infrastructure --startup-project src\Api

# Generar script SQL
dotnet ef migrations script --project src\Infrastructure --startup-project src\Api --output script.sql
```

### Opción 3: Script SQL Completo

Ejecuta el archivo `DatabaseScript.sql` en SQL Server Management Studio o Azure Data Studio.

---

## 📋 ORDEN DE CREACIÓN DE TABLAS

```
1. Roles               (sin dependencias)
2. EstadoReporte       (sin dependencias)
3. Areas               (sin dependencias)
4. Usuarios            (depende de Roles)
5. Reportes            (depende de Areas, EstadoReporte, Usuarios)
6. Condiciones         (depende de Reportes)
7. Actos               (depende de Reportes)
8. Evidencias          (depende de Reportes)
9. AnalisisIA          (depende de Reportes)
10. InformesIA         (sin dependencias)
11. RiesgosRepetitivos (sin dependencias)
12. Normativas         (sin dependencias)
13. LogsAuditoria      (sin dependencias)
```

---

## 🔍 VALIDACIÓN POST-MIGRACIÓN

Después de aplicar las migraciones, ejecuta estas consultas para verificar:

```sql
-- Verificar que las tablas existan
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Verificar datos seed de Roles
SELECT * FROM Roles;

-- Verificar datos seed de EstadoReporte
SELECT * FROM EstadoReporte;

-- Verificar índices de Usuarios
SELECT name, type_desc 
FROM sys.indexes 
WHERE object_id = OBJECT_ID('Usuarios');

-- Verificar índices de Reportes
SELECT name, type_desc 
FROM sys.indexes 
WHERE object_id = OBJECT_ID('Reportes');

-- Verificar estructura de AnalisisIA
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, COLUMN_DEFAULT
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AnalisisIA';
```

---

## 📐 DIAGRAMA ENTIDAD-RELACIÓN

```
┌─────────┐
│  Roles  │───┐
└─────────┘   │
              │ 1:N
              ▼
          ┌──────────┐
          │ Usuarios │───┐
          └──────────┘   │
                         │ 1:N (ReportadoPor)
┌───────┐                │
│ Areas │───┐            │
└───────┘   │ 1:N        │
            ▼            ▼
        ┌──────────────────┐
        │    Reportes      │◄───┐ 1:N
        └──────────────────┘    │
            │   │   │   │    ┌────────────────┐
            │   │   │   └────│ EstadoReporte  │
            │   │   │        └────────────────┘
            │   │   │
    ┌───────┘   │   └───────┐
    │ 1:N       │ 1:N       │ 1:N
    ▼           ▼           ▼
┌────────────┐ ┌──────────┐ ┌────────────┐
│Condiciones │ │  Actos   │ │ Evidencias │
└────────────┘ └──────────┘ └────────────┘
                │ 1:N
                ▼
            ┌────────────┐
            │ AnalisisIA │
            └────────────┘

[TABLAS INDEPENDIENTES]
┌──────────────────┐  ┌───────────────────┐  ┌────────────┐  ┌──────────────┐
│   InformesIA     │  │ RiesgosRepetitivos│  │ Normativas │  │LogsAuditoria │
└──────────────────┘  └───────────────────┘  └────────────┘  └──────────────┘
```

---

## 🔐 TIPOS DE DELETE (CASCADE vs RESTRICT)

### CASCADE DELETE (Eliminación en cascada)
- `Roles` → `Usuarios` ✅
- `Areas` → `Reportes` ✅
- `EstadoReporte` → `Reportes` ✅
- `Reportes` → `Condiciones, Actos, Evidencias, AnalisisIA` ✅

### RESTRICT DELETE (No permite eliminar)
- `Usuarios` → `Reportes` ❌ (Un usuario con reportes NO puede ser eliminado)

---

## 📊 CAMPOS CON VALORES POR DEFECTO

```sql
-- Timestamps automáticos
Usuarios.CreatedAt        → GETUTCDATE()
Usuarios.UpdatedAt        → GETUTCDATE()
Reportes.CreatedAt        → GETUTCDATE()
Reportes.UpdatedAt        → GETUTCDATE()
Evidencias.UploadedAt     → GETUTCDATE()
InformesIA.CreatedAt      → GETUTCDATE()
LogsAuditoria.Timestamp   → GETUTCDATE()

-- Valores iniciales
AnalisisIA.Status         → 'Pending'
AnalisisIA.AttemptCount   → 0
```

---

## 🎯 ÍNDICES CREADOS

### Índices Únicos
- `IX_Usuarios_Email` (UNIQUE)

### Índices de Búsqueda y Performance
- `IX_Usuarios_RolId`
- `IX_Reportes_FechaReporte`
- `IX_Reportes_AreaId`
- `IX_Reportes_EstadoReporteId`
- `IX_Reportes_ReportadoPorId`
- `IX_Condiciones_ReporteId`
- `IX_Actos_ReporteId`
- `IX_Evidencias_ReporteId`
- `IX_AnalisisIA_ReporteId`
- `IX_AnalisisIA_Status` ⭐ NUEVO

---

## ✅ CHECKLIST DE VERIFICACIÓN

- [x] Migración `20260208_InitialCreate.cs` existente
- [x] Migración `20260209_AddMissingFields.cs` creada
- [x] Script SQL completo generado
- [x] Documentación completa del esquema
- [x] Script PowerShell de aplicación
- [x] Todos los campos de las entidades sincronizados
- [x] Índices optimizados
- [x] Relaciones y foreign keys configuradas
- [x] Datos seed incluidos (Roles y Estados)
- [x] Valores por defecto configurados
- [x] Triggers para UpdatedAt (en script SQL)

---

## 🐛 SOLUCIÓN DE PROBLEMAS

### Error: "Unable to create an object of type 'SecurityReportDbContext'"
**Solución:** Verifica tu connection string en `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=SecurityReportDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Error: "dotnet ef not found"
**Solución:** Instala EF Core Tools:
```powershell
dotnet tool install --global dotnet-ef
```

### Error: "Login failed for user"
**Solución:** Revisa los permisos de SQL Server y la connection string.

---

## 📚 RECURSOS ADICIONALES

- 📄 `ESQUEMA_BASE_DATOS.md` - Documentación detallada de cada tabla
- 📄 `DatabaseScript.sql` - Script SQL completo
- 📄 `Apply-Migrations.ps1` - Script de automatización

---

## 🎉 ¡TODO LISTO!

Tu base de datos está completamente definida con:
- ✅ 13 tablas
- ✅ 6 relaciones foreign key
- ✅ 11 índices
- ✅ 6 registros seed
- ✅ Todos los campos sincronizados con las entidades

**Siguiente paso:** Ejecuta `.\Apply-Migrations.ps1` para crear la base de datos.

---

**Desarrollado para Security Report System v1.0**
**Última actualización:** 9 de Febrero de 2026
