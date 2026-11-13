using Microsoft.AspNetCore.Mvc;

namespace Credite.Web.Controllers;

public sealed class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Erro() => View();
}
