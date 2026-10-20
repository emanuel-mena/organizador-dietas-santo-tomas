using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace organizador_dietas_santo_tomas.Data;

public static class IdentitySeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config, ILogger logger)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();

        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        foreach (var role in new[] { AppRoles.Administrator, AppRoles.Doctor, AppRoles.Patient })
        {
            if (!await roles.RoleExistsAsync(role))
            {
                var result = await roles.CreateAsync(new IdentityRole(role));
                if (!result.Succeeded)
                    throw new InvalidOperationException($"No se pudo crear el rol {role}.");
            }
        }

        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        if (await users.GetUsersInRoleAsync(AppRoles.Administrator) is { Count: > 0 }) return;

        var email = config["SeedAdmin:Email"];
        var password = config["SeedAdmin:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogWarning("No existe un administrador. Configure SeedAdmin:Email y SeedAdmin:Password para crear el primero.");
            return;
        }

        if (await users.FindByEmailAsync(email) is not null)
            throw new InvalidOperationException("El correo del administrador inicial ya pertenece a otra cuenta.");

        var admin = new ApplicationUser { UserName = email, Email = email, FullName = "Administrador" };
        var created = await users.CreateAsync(admin, password);
        if (!created.Succeeded)
            throw new InvalidOperationException("No se pudo crear el administrador inicial: " + string.Join("; ", created.Errors.Select(e => e.Description)));
        var assigned = await users.AddToRoleAsync(admin, AppRoles.Administrator);
        if (!assigned.Succeeded)
            throw new InvalidOperationException("No se pudo asignar el rol al administrador inicial.");
    }
}
