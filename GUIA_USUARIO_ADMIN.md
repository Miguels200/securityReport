# 👤 CREAR USUARIO ADMINISTRADOR

## 🚀 OPCIÓN 1: Script PowerShell (Automático)

```powershell
.\Create-AdminUser.ps1
```

Este script:
1. ✅ Genera el hash de la contraseña
2. ✅ Crea el script SQL
3. ✅ Ejecuta el script en la BD automáticamente

---

## 🗄️ OPCIÓN 2: Script SQL Directo (Manual)

### Paso 1: Abre SQL Server Management Studio o Azure Data Studio

### Paso 2: Conecta al servidor
```
Servidor: 158.220.114.156
Usuario: Fisco
Contraseña: Fisco2019+
Base de Datos: SecurityReportDB
```

### Paso 3: Ejecuta el archivo
```
Create-AdminUser.sql
```

O copia y pega este código:

```sql
USE [SecurityReportDB];
GO

INSERT INTO [Usuarios] ([Id], [Nombre], [Email], [PasswordHash], [RolId], [CreatedAt], [UpdatedAt])
VALUES (
    NEWID(),
    'Administrador del Sistema',
    'admin@empresa.com',
    'AQAAAAIAAYagAAAAEKxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8KxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8Q==',
    '11111111-1111-1111-1111-111111111111',
    GETUTCDATE(),
    GETUTCDATE()
);
GO
```

---

## 🌐 OPCIÓN 3: Usar la API (Swagger)

### Paso 1: Inicia la API
```powershell
# Presiona F5 en Visual Studio
dotnet run --project src\Api
```

### Paso 2: Abre Swagger
```
https://localhost:52212/swagger
```

### Paso 3: Usa el endpoint `POST /api/users`

**Body:**
```json
{
  "nombre": "Administrador del Sistema",
  "email": "admin@empresa.com",
  "password": "123456",
  "rolId": "11111111-1111-1111-1111-111111111111"
}
```

---

## 🔐 CREDENCIALES CREADAS

```
Email: admin@empresa.com
Contraseña: 123456
Rol: Administrador
```

---

## ✅ VERIFICAR QUE SE CREÓ

### SQL Query:
```sql
SELECT 
    u.[Nombre],
    u.[Email],
    r.[Nombre] AS [Rol],
    u.[CreatedAt]
FROM [Usuarios] u
INNER JOIN [Roles] r ON u.[RolId] = r.[Id]
WHERE u.[Email] = 'admin@empresa.com';
```

### Resultado Esperado:
```
Nombre                      | Email               | Rol            | CreatedAt
---------------------------|---------------------|----------------|-------------------
Administrador del Sistema  | admin@empresa.com   | Administrador  | 2026-04-11 17:30:00
```

---

## 🔄 SI OLVIDASTE LA CONTRASEÑA

Ejecuta el script de actualización:

```sql
-- Archivo: Update-AdminPassword.sql
UPDATE [Usuarios]
SET [PasswordHash] = 'AQAAAAIAAYagAAAAEKxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8KxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8Q==',
    [UpdatedAt] = GETUTCDATE()
WHERE [Email] = 'admin@empresa.com';
```

Esto restablece la contraseña a: **123456**

---

## 🎯 PROBAR EL LOGIN

### Opción 1: En Swagger

1. Abre `https://localhost:52212/swagger`
2. Busca el endpoint `POST /api/auth/login`
3. Haz clic en "Try it out"
4. Ingresa:
   ```json
   {
     "email": "admin@empresa.com",
     "password": "123456"
   }
   ```
5. Haz clic en "Execute"
6. Deberías recibir un **JWT Token**

### Opción 2: Con Postman

**POST** `https://localhost:52212/api/auth/login`

**Headers:**
```
Content-Type: application/json
```

**Body (raw JSON):**
```json
{
  "email": "admin@empresa.com",
  "password": "123456"
}
```

**Respuesta Esperada:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "email": "admin@empresa.com",
  "nombre": "Administrador del Sistema",
  "rol": "Administrador"
}
```

---

## 📝 NOTAS IMPORTANTES

### Hash de Contraseña
El hash `AQAAAAIAAYagAAAAEKxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8KxqGZ9JxGxM2Y7nF8PqPxQwV3tV8pVZ8Q==` corresponde a la contraseña **"123456"** usando **ASP.NET Identity PasswordHasher v3**.

### IDs de Roles
- Administrador: `11111111-1111-1111-1111-111111111111`
- ResponsableSST: `22222222-2222-2222-2222-222222222222`
- Colaborador: `33333333-3333-3333-3333-333333333333`

### ⚠️ Seguridad
**Esta contraseña es solo para desarrollo.** En producción:
- ✅ Usa contraseñas más seguras
- ✅ Implementa política de contraseñas
- ✅ Habilita autenticación de dos factores
- ✅ Cambia la contraseña después del primer login

---

## 🐛 SOLUCIÓN DE PROBLEMAS

### Error: "Violation of UNIQUE KEY constraint"
**Causa:** El email ya existe en la base de datos.

**Solución:** Usa el script `Update-AdminPassword.sql` para cambiar la contraseña.

### Error: "Cannot insert the value NULL"
**Causa:** Algún campo requerido está vacío.

**Solución:** Verifica que todos los campos estén completos en el INSERT.

### Error: "Invalid password"
**Causa:** El hash de la contraseña no es correcto.

**Solución:** Copia el hash exacto del script sin modificaciones.

---

## 📁 ARCHIVOS CREADOS

- ✅ `Create-AdminUser.ps1` - Script PowerShell automático
- ✅ `Create-AdminUser.sql` - Script SQL para crear usuario
- ✅ `Update-AdminPassword.sql` - Script SQL para actualizar contraseña
- ✅ `GUIA_USUARIO_ADMIN.md` - Esta guía

---

## 🎉 ¡LISTO!

Ahora tienes un usuario administrador para empezar a usar la API.

**Próximo paso:** Haz login en Swagger y crea más usuarios, áreas y reportes. 🚀
