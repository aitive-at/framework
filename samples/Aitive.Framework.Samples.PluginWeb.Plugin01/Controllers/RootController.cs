using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Aitive.Framework.Samples.PluginWeb.Plugin01.Controllers;

[Route("test")]
public class RootController(TestService testService) : Controller
{
    [HttpGet]
    public ActionResult Index()
    {
        return View(testService);
    }
}
