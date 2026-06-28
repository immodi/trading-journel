using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("/")]
public class IndexController(
    IWebHostEnvironment env, 
    ILogger<IndexController> logger
    ) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        logger.LogInformation("Starting Main Application...");
        
        if (env.IsDevelopment())
        {
            return Ok("this is a development environment, run the frontend application separately.");
        }
        
        var path = Path.Combine(env.WebRootPath, "index.html");
        return PhysicalFile(path, "text/html");;
    }
}