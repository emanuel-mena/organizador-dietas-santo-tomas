using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace organizador_dietas_santo_tomas.Controllers;

[AllowAnonymous]
public class DesignSystemController(IWebHostEnvironment environment) : Controller
{
    [HttpGet]
    public IActionResult Index() => environment.IsDevelopment() ? View() : NotFound();
}
