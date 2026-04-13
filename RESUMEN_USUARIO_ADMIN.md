# 🎉 SCRIPTS DE USUARIO ADMINISTRADOR CREADOS

## ✅ ARCHIVOS CREADOS

1. ✅ **`Create-AdminUser.ps1`** - Script PowerShell automático
2. ✅ **`Create-AdminUser.sql`** - Script SQL directo
3. ✅ **`Update-AdminPassword.sql`** - Script para restablecer contraseña
4. ✅ **`GUIA_USUARIO_ADMIN.md`** - Guía completa de uso

---

## 🚀 FORMA MÁS RÁPIDA (1 COMANDO)

### Opción 1: PowerShell
```powershell
.\Create-AdminUser.ps1
```

### Opción 2: SQL Directo
Ejecuta en SQL Server Management Studio:
```
Create-AdminUser.sql
```

---

## 🔐 CREDENCIALES

```
Email: admin@empresa.com
Contraseña: 123456
Rol: Administrador
```

---

## 📋 QUÉ HACE CADA SCRIPT

### `Create-AdminUser.ps1` (PowerShell)
- ✅ Genera hash de contraseña automáticamente
- ✅ Crea el script SQL
- ✅ Lo ejecuta en la base de datos
- ✅ Verifica que se creó correctamente

### `Create-AdminUser.sql` (SQL)
- ✅ Inserta el usuario administrador
- ✅ Verifica que el email no exista
- ✅ Muestra mensaje de éxito
- ✅ Lista los usuarios administradores

### `Update-AdminPassword.sql` (SQL)
- ✅ Restablece la contraseña a "123456"
- ✅ Útil si olvidaste la contraseña
- ✅ Actualiza el campo `UpdatedAt`

---

## 🎯 SIGUIENTE PASO

### 1. Ejecuta el script:
```powershell
.\Create-AdminUser.ps1
```

### 2. Inicia la API:
Presiona **F5** en Visual Studio

### 3. Abre Swagger:
```
https://localhost:52212/swagger
```

### 4. Haz Login:
Usa el endpoint `POST /api/auth/login` con:
```json
{
  "email": "admin@empresa.com",
  "password": "123456"
}
```

### 5. Copia el Token:
Recibirás un JWT token. Cópialo.

### 6. Autoriza en Swagger:
- Haz clic en el botón **"Authorize"** (candado verde arriba a la derecha)
- Pega el token en el campo "Value"
- Haz clic en "Authorize"

### 7. ¡Listo!
Ahora puedes usar todos los endpoints de la API. 🎉

---

## 📚 DOCUMENTACIÓN

Lee `GUIA_USUARIO_ADMIN.md` para más detalles sobre:
- Cómo verificar que se creó
- Cómo probar el login
- Solución de problemas
- IDs de roles disponibles

---

**¡Ahora tienes un usuario admin para empezar! 🚀**
