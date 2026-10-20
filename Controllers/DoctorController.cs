using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using organizador_dietas_santo_tomas.Data;
using organizador_dietas_santo_tomas.Models;
using organizador_dietas_santo_tomas.Services;

namespace organizador_dietas_santo_tomas.Controllers;

[Authorize(Roles = AppRoles.Doctor)]
public class DoctorController(ApplicationDbContext db, UserManager<ApplicationUser> users,
    UserCreationService creator) : Controller
{
    public IActionResult Index() => View();

    public async Task<IActionResult> Patients()
    {
        var doctorId = users.GetUserId(User)!;
        var rows = await db.DoctorPatients.Where(link => link.DoctorId == doctorId)
            .OrderBy(link => link.Patient.FullName)
            .Select(link => new UserListItem(link.PatientId, link.Patient.FullName, link.Patient.Email ?? "",
                AppRoles.Patient, link.Patient.CreatedByDoctorId == doctorId))
            .ToListAsync();
        return View(rows);
    }

    [HttpGet]
    public IActionResult CreatePatient() => View(new CreateUserViewModel { Role = AppRoles.Patient });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreatePatient(CreateUserViewModel model)
    {
        model.Role = AppRoles.Patient;
        ModelState.Remove(nameof(model.Role));
        if (!ModelState.IsValid) return View(model);

        var doctorId = users.GetUserId(User)!;
        var result = await creator.CreateAsync(model.FullName, model.Email, model.Password,
            AppRoles.Patient, [doctorId], doctorId);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }
        TempData["Message"] = "Paciente creado y asociado correctamente.";
        return RedirectToAction(nameof(Patients));
    }

    [HttpGet]
    public IActionResult AssociatePatient() => View(new AssociatePatientViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AssociatePatient(AssociatePatientViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var patient = await users.FindByEmailAsync(model.Email.Trim());
        if (patient is null || !await users.IsInRoleAsync(patient, AppRoles.Patient))
        {
            ModelState.AddModelError(nameof(model.Email), "No existe un paciente con ese correo.");
            return View(model);
        }

        var doctorId = users.GetUserId(User)!;
        if (await db.DoctorPatients.AnyAsync(link => link.DoctorId == doctorId && link.PatientId == patient.Id))
        {
            ModelState.AddModelError(nameof(model.Email), "Este paciente ya está asociado a su cuenta.");
            return View(model);
        }
        db.DoctorPatients.Add(new DoctorPatient { DoctorId = doctorId, PatientId = patient.Id });
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            ModelState.AddModelError(nameof(model.Email), "No se pudo crear la asociación; puede que ya exista.");
            return View(model);
        }
        TempData["Message"] = "Paciente asociado correctamente.";
        return RedirectToAction(nameof(Patients));
    }
}
