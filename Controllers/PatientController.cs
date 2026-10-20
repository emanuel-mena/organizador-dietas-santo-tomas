using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using organizador_dietas_santo_tomas.Data;
using organizador_dietas_santo_tomas.Models;

namespace organizador_dietas_santo_tomas.Controllers;

[Authorize(Roles = AppRoles.Patient)]
public class PatientController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var patientId = users.GetUserId(User)!;
        var doctors = await db.DoctorPatients.Where(link => link.PatientId == patientId)
            .OrderBy(link => link.Doctor.FullName)
            .Select(link => new UserListItem(link.DoctorId, link.Doctor.FullName, link.Doctor.Email ?? "", AppRoles.Doctor))
            .ToListAsync();
        return View(doctors);
    }
}
