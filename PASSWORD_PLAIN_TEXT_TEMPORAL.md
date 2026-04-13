# ⚠️ CONTRASEÑAS EN TEXTO PLANO (SOLO PARA PRUEBAS)

## ⚠️ ADVERTENCIA IMPORTANTE

**Las contraseñas ahora se guardan en texto plano (sin hash) para facilitar las pruebas de desarrollo.**  
**NO USAR ESTE CÓDIGO EN PRODUCCIÓN**

---

## Cambios Realizados

Se han comentado todas las validaciones de hash de contraseñas en los siguientes archivos:

### 1. AuthController.cs
**Login (línea ~31-42):**
```csharp
// TEMPORAL: Sin hash para pruebas - contraseña en texto plano
if (user.PasswordHash != req.Password) return Unauthorized();

// COMENTADO:
// var res = _hasher.Verify(user.PasswordHash, req.Password);
// if (res != 1) return Unauthorized();
```

**ChangePassword (línea ~58-69):**
```csharp
// TEMPORAL: Sin hash para pruebas - contraseña en texto plano
if (user.PasswordHash != req.CurrentPassword)
{
    return BadRequest(new { message = "Contraseña actual inválida" });
}

user.PasswordHash = req.NewPassword; // TEMPORAL: Sin hash

// COMENTADO:
// var verify = _hasher.Verify(user.PasswordHash, req.CurrentPassword);
// user.PasswordHash = _hasher.Hash(req.NewPassword);
```

### 2. UsersController.cs
**ResetPassword (línea ~113-120):**
```csharp
user.PasswordHash = "Temporal123!"; // TEMPORAL: Sin hash

// COMENTADO:
// user.PasswordHash = _hasher.Hash("Temporal123!");
```

### 3. CreateUserHandler.cs
**Handle (línea ~29):**
```csharp
PasswordHash = request.Password, // TEMPORAL: Sin hash para pruebas

// COMENTADO:
// PasswordHash = _hasher.Hash(request.Password),
```

### 4. Program.cs
**Seed Data (línea ~287):**
```csharp
PasswordHash = "admin123", // TEMPORAL: Sin hash para pruebas

// COMENTADO:
// PasswordHash = hasher.Hash("admin123"),
```

---

## Usuario Administrador Actualizado

Se actualizó la contraseña del usuario administrador existente en la base de datos:

| Campo | Valor |
|-------|-------|
| Email | admin@empresa.com |
| Contraseña | 123456 |
| PasswordHash (BD) | 123456 |

---

## Comportamiento Actual

### Login
- Email: `admin@empresa.com`
- Password: `123456`
- La contraseña se compara directamente: `user.PasswordHash != req.Password`

### Crear Usuario
- La contraseña se guarda tal cual: `PasswordHash = request.Password`
- Si escribes "123456" en la UI → Se guarda "123456" en la BD

### Cambiar Contraseña
- Validación: `user.PasswordHash != req.CurrentPassword`
- Nueva contraseña: `user.PasswordHash = req.NewPassword`

### Resetear Contraseña
- Contraseña temporal: "Temporal123!"
- Se guarda directamente sin hash

---

## Pruebas Recomendadas

1. **Login con admin:**
   ```json
   POST /api/auth/login
   {
     "email": "admin@empresa.com",
     "password": "123456"
   }
   ```

2. **Crear nuevo usuario:**
   ```json
   POST /api/users
   {
     "nombre": "Usuario Prueba",
     "email": "prueba@empresa.com",
     "password": "123456",
     "rolId": "11111111-1111-1111-1111-111111111111"
   }
   ```

3. **Verificar en BD:**
   ```sql
   SELECT Id, Nombre, Email, PasswordHash FROM [dbo].[Usuarios];
   ```
   Deberías ver las contraseñas en texto plano (ej: "123456")

---

## Restaurar Hash de Contraseñas (Futuro)

Para volver a usar hash de contraseñas:

1. **Descomentar el código** en los 4 archivos modificados
2. **Comentar las líneas TEMPORAL**
3. **Actualizar contraseñas existentes** en la BD con hash:
   ```csharp
   var hasher = new PasswordHasher<string>();
   var hash = hasher.HashPassword(null, "123456");
   ```

---

## Archivos Modificados

1. ✅ `src/Api/Controllers/AuthController.cs` - Login y ChangePassword
2. ✅ `src/Api/Controllers/UsersController.cs` - ResetPassword
3. ✅ `src/Application/Handlers/CreateUserHandler.cs` - CreateUser
4. ✅ `src/Api/Program.cs` - Seed data

---

## Estado de la Base de Datos

```sql
-- Usuario admin actualizado
UPDATE [dbo].[Usuarios] 
SET PasswordHash = '123456' 
WHERE Email = 'admin@empresa.com';

-- Verificar
SELECT Id, Nombre, Email, PasswordHash 
FROM [dbo].[Usuarios] 
WHERE Email = 'admin@empresa.com';
```

**Resultado:**
- Id: FBA09DCF-B915-41A0-9185-84F18F6B894D
- Nombre: Administrador del Sistema
- Email: admin@empresa.com
- PasswordHash: **123456**

---

**Fecha:** 2026-02-09  
**Estado:** ✅ Aplicado  
**Tipo:** Temporal (solo para pruebas de desarrollo)  
**Compilación:** ✅ Exitosa
