using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("/")]
public class IndexController(IWebHostEnvironment env, ILogger<IndexController> logger) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        if (env.IsDevelopment())
        {
            return Ok("This is the development environment. Run the frontend application separately.");
        }

        var webRoot = env.WebRootPath;
        var index = string.IsNullOrEmpty(webRoot) ? null : Path.Combine(webRoot, "index.html");

        if (index is null || !System.IO.File.Exists(index))
        {
            logger.LogWarning("SPA entry point not found under wwwroot ({WebRoot}).", webRoot);
            return NotFound(new { message = "The frontend build was not found. Publish with the frontend build step enabled (do not pass -p:SkipFrontendBuild=true)." });
        }

        return PhysicalFile(index, "text/html");
    }
}
