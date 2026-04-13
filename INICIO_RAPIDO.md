# 🚀 INICIO RÁPIDO - SECURITY REPORT

## ✅ CONFIGURACIÓN COMPLETA

Tu proyecto ya está configurado para usar:

### 🗄️ Servidor SQL Server
```
📍 Servidor: 158.220.114.156
📊 Base de Datos: SecurityReportDB (se creará automáticamente)
👤 Usuario: Fisco
🔑 Password: Fisco2019+
```

---

## 🎯 PASOS PARA EMPEZAR (3 PASOS)

### 1️⃣ Crear la Base de Datos

```powershell
.\Create-Database.ps1
```

**Esto hará:**
- ✅ Crear la base de datos SecurityReportDB
- ✅ Crear las 13 tablas
- ✅ Insertar datos iniciales (Roles y Estados)

---

### 2️⃣ Ejecutar la API

Presiona **F5** en Visual Studio

O en terminal:
```powershell
dotnet run --project src\Api
```

---

### 3️⃣ Probar con Swagger

Abre tu navegador en:
```
https://localhost:7xxx/swagger
```

---

## 📋 DATOS SEED DISPONIBLES

Después de crear la BD, tendrás:

### Roles:
- Administrador
- ResponsableSST
- Colaborador

### Estados de Reporte:
- Abierto
- EnProgreso
- Cerrado

---

## ⚡ COMANDOS ÚTILES

### Ver migraciones disponibles:
```powershell
dotnet ef migrations list --project src\Infrastructure --startup-project src\Api
```

### Aplicar migraciones:
```powershell
dotnet ef database update --project src\Infrastructure --startup-project src\Api
```

### Generar script SQL:
```powershell
dotnet ef migrations script --project src\Infrastructure --startup-project src\Api --output migration.sql
```

### Eliminar base de datos (CUIDADO):
```powershell
dotnet ef database drop --project src\Infrastructure --startup-project src\Api --force
```

---

## 🔍 VERIFICAR LA INSTALACIÓN

### En SQL Server:
```sql
USE SecurityReportDB;

-- Ver todas las tablas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE';

-- Ver roles
SELECT * FROM Roles;

-- Ver estados
SELECT * FROM EstadoReporte;
```

---

## 📚 DOCUMENTACIÓN COMPLETA

- 📄 `CONFIGURACION_APLICADA.md` - Detalles de configuración
- 📄 `ESQUEMA_BASE_DATOS.md` - Esquema completo de las 13 tablas
- 📄 `README_MIGRACIONES.md` - Guía de migraciones
- 📄 `DatabaseScript.sql` - Script SQL completo (alternativo)

---

## 🎉 ¡LISTO!

**AHORA EJECUTA:**
```powershell
.\Create-Database.ps1
```

Y presiona **F5** para iniciar la API 🚀
