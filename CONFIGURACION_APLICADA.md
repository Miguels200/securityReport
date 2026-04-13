# ⚙️ CONFIGURACIÓN APLICADA - SECURITY REPORT

## ✅ ARCHIVOS CREADOS

### 1. Configuración de la Aplicación
- ✅ `src\Api\appsettings.json` - Configuración base
- ✅ `src\Api\appsettings.Development.json` - Configuración de desarrollo

### 2. Script de Creación
- ✅ `Create-Database.ps1` - Script automático para crear la BD

---

## 🗄️ CONFIGURACIÓN DE BASE DE DATOS

### Servidor SQL Server
```
Servidor: 158.220.114.156
Base de Datos: SecurityReportDB
Usuario: Fisco
Password: Fisco2019+
```

### Connection String
```
Data Source=158.220.114.156;Initial Catalog=SecurityReportDB;Persist Security Info=True;User ID=Fisco;Password=Fisco2019+;MultipleActiveResultSets=True;TrustServerCertificate=True
```

---

## 🔐 CONFIGURACIÓN JWT

```json
{
  "JWT_SECRET": "SecurityReport_Super_Secret_Key_2026_1234567890_ABCDEFGHIJKLMNOP",
  "JWT_ISSUER": "SecurityReport",
  "JWT_AUDIENCE": "SecurityReport"
}
```

---

## 🌐 CONFIGURACIÓN CORS

Orígenes permitidos para desarrollo:
- `http://localhost:4200` (Angular)
- `http://localhost:5173` (Vite/React)

---

## 🚀 CÓMO CREAR LA BASE DE DATOS

### Opción 1: Script PowerShell (MÁS FÁCIL) ⭐

```powershell
.\Create-Database.ps1
```

Este script:
1. ✅ Verifica la conexión
2. ✅ Aplica todas las migraciones
3. ✅ Crea las 13 tablas
4. ✅ Inserta datos seed (Roles y Estados)

---

### Opción 2: Comando Manual

```powershell
dotnet ef database update --project src\Infrastructure --startup-project src\Api
```

---

## 📊 QUÉ SE CREARÁ EN EL SERVIDOR

### En el servidor: `158.220.114.156`

Se creará la base de datos **SecurityReportDB** con:

#### 13 Tablas:
1. ✅ Roles (3 registros)
2. ✅ EstadoReporte (3 registros)
3. ✅ Areas
4. ✅ Usuarios
5. ✅ Reportes
6. ✅ Condiciones
7. ✅ Actos
8. ✅ Evidencias
9. ✅ AnalisisIA
10. ✅ InformesIA
11. ✅ RiesgosRepetitivos
12. ✅ Normativas
13. ✅ LogsAuditoria

#### Datos Iniciales (Seed):

**Roles:**
- 🔐 Administrador
- 👷 ResponsableSST
- 👤 Colaborador

**Estados de Reporte:**
- 📝 Abierto
- ⏳ EnProgreso
- ✅ Cerrado

---

## 🔍 VERIFICACIÓN POST-CREACIÓN

Después de ejecutar el script, verifica en SQL Server:

```sql
-- Conectarse al servidor
USE SecurityReportDB;

-- Ver todas las tablas
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;

-- Verificar roles
SELECT * FROM Roles;

-- Verificar estados
SELECT * FROM EstadoReporte;
```

Deberías ver:
- ✅ 13 tablas creadas
- ✅ 3 roles insertados
- ✅ 3 estados insertados

---

## 📁 ESTRUCTURA DE CONFIGURACIÓN

```
src/Api/
├── appsettings.json              ← Configuración base (sin secretos)
└── appsettings.Development.json  ← Configuración de desarrollo (con conexión)
```

### Variables de configuración usadas en `Program.cs`:

| Variable | Ubicación | Valor |
|----------|-----------|-------|
| `DB_CONNECTION` | appsettings.Development.json | Connection string completa |
| `JWT_SECRET` | appsettings.Development.json | Clave secreta para tokens |
| `JWT_ISSUER` | appsettings.Development.json | "SecurityReport" |
| `JWT_AUDIENCE` | appsettings.Development.json | "SecurityReport" |
| `CORS_ORIGINS` | appsettings.Development.json | URLs permitidas |

---

## 🔒 SEGURIDAD - IMPORTANTE

### ⚠️ Para PRODUCCIÓN:

**NO incluyas las credenciales en `appsettings.json`**

En producción, usa:

#### Opción 1: Variables de Entorno
```powershell
$env:DB_CONNECTION = "tu-connection-string"
$env:JWT_SECRET = "tu-secret"
```

#### Opción 2: Azure Key Vault
El código ya está preparado para usar Key Vault:
```csharp
var kvUri = builder.Configuration["AZURE_KEYVAULT_URI"];
```

---

## 🎯 SIGUIENTE PASO

### 1️⃣ Ejecuta el script de creación:

```powershell
.\Create-Database.ps1
```

### 2️⃣ Verifica que se creó correctamente

Conéctate a SQL Server y verifica:
```sql
USE SecurityReportDB;
SELECT * FROM Roles;
```

### 3️⃣ Ejecuta la API

Presiona **F5** en Visual Studio o ejecuta:
```powershell
dotnet run --project src\Api
```

### 4️⃣ Accede a Swagger

Abre en tu navegador:
```
https://localhost:7xxx/swagger
```

---

## 🐛 SOLUCIÓN DE PROBLEMAS

### Error: "Login failed for user 'Fisco'"
✅ Verifica que el usuario tenga permisos en el servidor  
✅ Asegúrate de que el firewall permita la conexión

### Error: "The server was not found"
✅ Verifica la conectividad al servidor 158.220.114.156  
✅ Revisa que el puerto 1433 esté abierto

### Error: "Cannot create database"
✅ Verifica que el usuario 'Fisco' tenga permisos de CREATE DATABASE  
✅ O crea manualmente: `CREATE DATABASE SecurityReportDB;`

---

## 📝 COMPARACIÓN CON TU PROYECTO DE FINCA

| Característica | Finca | SecurityReport |
|----------------|-------|----------------|
| **Servidor** | 158.220.114.156 | ✅ IGUAL |
| **Usuario** | Fisco | ✅ IGUAL |
| **Password** | Fisco2019+ | ✅ IGUAL |
| **Base de Datos** | InvFinBDPru | ❌ SecurityReportDB |
| **JWT Key** | FincaApp_Super... | ❌ SecurityReport_Super... |
| **Issuer** | FincaApp | ❌ SecurityReport |
| **Audience** | FincaApp | ❌ SecurityReport |

---

## ✅ CHECKLIST DE CONFIGURACIÓN

- [x] `appsettings.json` creado
- [x] `appsettings.Development.json` creado con DB_CONNECTION
- [x] JWT configurado
- [x] CORS configurado
- [x] Script de creación de BD listo
- [x] Migraciones preparadas
- [ ] ⏳ Ejecutar `Create-Database.ps1`
- [ ] ⏳ Verificar que se crearon las tablas
- [ ] ⏳ Probar la API con F5

---

## 🎉 ¡LISTO PARA USAR!

Tu aplicación ahora usará el mismo servidor que tu proyecto de Finca pero con una base de datos separada: **SecurityReportDB**

**Ejecuta:** `.\Create-Database.ps1` para crear todo automáticamente.

---

**Configuración completada:** 9 de Febrero de 2026  
**Servidor:** 158.220.114.156  
**Base de Datos:** SecurityReportDB
