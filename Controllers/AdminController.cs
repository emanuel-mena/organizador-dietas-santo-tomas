using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using organizador_dietas_santo_tomas.Data;
using organizador_dietas_santo_tomas.Models;
using organizador_dietas_santo_tomas.Services;

namespace organizador_dietas_santo_tomas.Controllers;

[Authorize(Roles = AppRoles.Administrator)]
public class AdminController(ApplicationDbContext db, UserCreationService creator) : Controller
{
    public IActionResult Index() => View();

    public async Task<IActionResult> Users()
    {
        var rows = await (from user in db.Users
                          join userRole in db.UserRoles on user.Id equals userRole.UserId
                          join role in db.Roles on userRole.RoleId equals role.Id
                          orderby user.FullName
                          select new UserListItem(user.Id, user.FullName, user.Email ?? "", role.Name ?? ""))
            .ToListAsync();
        return View(rows);
    }

    [HttpGet]
    public async Task<IActionResult> CreateUser() => View(await PopulateDoctors(new CreateUserViewModel()));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateUser(CreateUserViewModel model)
    {
        if (model.Role is not (AppRoles.Administrator or AppRoles.Doctor or AppRoles.Patient))
            ModelState.AddModelError(nameof(model.Role), "Seleccione un rol válido.");
        if (!ModelState.IsValid) return View(await PopulateDoctors(model));

        var result = await creator.CreateAsync(model.FullName, model.Email, model.Password, model.Role, model.DoctorIds);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View(await PopulateDoctors(model));
        }
        TempData["Message"] = "Usuario creado correctamente.";
        return RedirectToAction(nameof(Users));
    }

    private async Task<CreateUserViewModel> PopulateDoctors(CreateUserViewModel model)
    {
        var doctorRoleId = await db.Roles.Where(r => r.Name == AppRoles.Doctor).Select(r => r.Id).SingleAsync();
        model.AvailableDoctors = await (from user in db.Users
                                        join userRole in db.UserRoles on user.Id equals userRole.UserId
                                        where userRole.RoleId == doctorRoleId
                                        orderby user.FullName
                                        select new UserListItem(user.Id, user.FullName, user.Email ?? "", AppRoles.Doctor))
            .ToListAsync();
        return model;
    }
}
