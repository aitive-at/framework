using Microsoft.AspNetCore.Mvc;

namespace Aitive.Framework.Samples.PluginWeb;

[Route("/")]
public class TestController : ControllerBase
{
    [HttpGet]
    public ActionResult Get()
    {
        return Ok("ROOT");
    }
}
