using Microsoft.AspNetCore.Mvc;
using System;

namespace FilesAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow,
                version = "1.0.0",
                environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                database = Environment.GetEnvironmentVariable("USE_EMBEDDED_DATABASE") == "true" ? "LiteDB" : "MongoDB"
            });
        }
    }
}
