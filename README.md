# Organizador de dietas Santo Tomás

Aplicación ASP.NET Core MVC con Identity, EF Core y PostgreSQL. Tiene tres roles: Administrador, Médico y Paciente.

## Configuración local

Se requiere .NET 10 y una base de datos PostgreSQL. Configure los secretos del proyecto sin incluir contraseñas en Git:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=organizador_dietas_santo_tomas;Username=USUARIO;Password=CLAVE"
dotnet user-secrets set "SeedAdmin:Email" "admin@ejemplo.com"
dotnet user-secrets set "SeedAdmin:Password" "CONTRASEÑA_SEGURA"
dotnet run
```

Al iniciar, la aplicación aplica las migraciones pendientes, crea los roles y, si todavía no hay administrador, crea la cuenta configurada. Los secretos también se pueden proporcionar mediante variables de entorno `ConnectionStrings__DefaultConnection`, `SeedAdmin__Email` y `SeedAdmin__Password`.

Para aplicar la migración manualmente: `dotnet ef database update`.

## Uso

- En **Registrarse**, una persona elige Médico o Paciente. El acceso queda disponible de inmediato.
- El administrador puede ver todos los usuarios y crear cuentas de cualquier rol. Al crear un paciente puede asignarle varios médicos o ninguno.
- Un médico puede crear un paciente nuevo y asociarlo automáticamente, o asociar por correo exacto una cuenta de paciente existente. Su lista muestra todos sus pacientes asociados e identifica los que creó.
- El paciente ve sus médicos asociados en su pantalla inicial.

Las contraseñas iniciales de cuentas creadas por administrador o médico se introducen en el formulario correspondiente.
