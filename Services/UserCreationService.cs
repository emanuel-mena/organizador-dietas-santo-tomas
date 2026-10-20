using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using organizador_dietas_santo_tomas.Data;

namespace organizador_dietas_santo_tomas.Services;

public class UserCreationService(ApplicationDbContext db, UserManager<ApplicationUser> users)
{
    public async Task<IdentityResult> CreateAsync(string fullName, string email, string password, string role,
        IEnumerable<string>? doctorIds = null, string? createdByDoctorId = null)
    {
        if (role is not (AppRoles.Administrator or AppRoles.Doctor or AppRoles.Patient))
            return IdentityResult.Failed(new IdentityError { Description = "Rol no válido." });

        var selectedDoctors = (doctorIds ?? []).Distinct().ToArray();
        if (role != AppRoles.Patient && selectedDoctors.Length > 0)
            return IdentityResult.Failed(new IdentityError { Description = "Solo los pacientes pueden tener médicos asociados." });

        if (selectedDoctors.Length > 0)
        {
            var doctorRoleId = await db.Roles.Where(r => r.Name == AppRoles.Doctor).Select(r => r.Id).SingleAsync();
            var validCount = await db.UserRoles.CountAsync(ur => ur.RoleId == doctorRoleId && selectedDoctors.Contains(ur.UserId));
            if (validCount != selectedDoctors.Length)
                return IdentityResult.Failed(new IdentityError { Description = "Se seleccionó un médico no válido." });
        }

        await using var transaction = await db.Database.BeginTransactionAsync();
        var normalizedEmail = email.Trim();
        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = fullName.Trim(),
            CreatedByDoctorId = role == AppRoles.Patient ? createdByDoctorId : null
        };
        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded) return result;

        result = await users.AddToRoleAsync(user, role);
        if (!result.Succeeded) return result;

        foreach (var doctorId in selectedDoctors)
            db.DoctorPatients.Add(new DoctorPatient { DoctorId = doctorId, PatientId = user.Id });
        await db.SaveChangesAsync();
        await transaction.CommitAsync();
        return IdentityResult.Success;
    }
}
