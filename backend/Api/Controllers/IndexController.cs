using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("/")]
public class IndexController(IWebHostEnvironment env) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        if (env.IsDevelopment())
        {
            return Ok("This is the development environment. Run the frontend application separately.");
        }

        var path = Path.Combine(env.WebRootPath, "index.html");
        return PhysicalFile(path, "text/html");
    }
}