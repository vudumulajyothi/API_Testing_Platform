using Microsoft.AspNetCore.Mvc;

namespace TestPilot.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult CheckHealth()
    {
        return Ok(new
        {
            status = "Healthy",
            service = "TestPilot API",
            version = "1.0.0",
            timestamp = DateTime.UtcNow
        });
    }
}
