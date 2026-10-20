using Microsoft.AspNetCore.Mvc;
using organizador_dietas_santo_tomas.Data;
using organizador_dietas_santo_tomas.Models;
using System.Diagnostics;

namespace organizador_dietas_santo_tomas.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated != true) return View();
            if (User.IsInRole(AppRoles.Administrator)) return RedirectToAction("Index", "Admin");
            if (User.IsInRole(AppRoles.Doctor)) return RedirectToAction("Index", "Doctor");
            if (User.IsInRole(AppRoles.Patient)) return RedirectToAction("Index", "Patient");
            return RedirectToAction("AccessDenied", "Account");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
