using Microsoft.AspNetCore.Mvc;

namespace TelesEducacao.Auth.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var result = new
        {
            Message = "API está funcionando!",
            DateTime = DateTime.Now,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
        };

        return Ok(result);
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        var result = new
        {
            Status = "Healthy",
            Version = "1.0.0",
            Timestamp = DateTime.UtcNow
        };

        return Ok(result);
    }

    [HttpGet("info")]
    public IActionResult Info()
    {
        var result = new
        {
            ServiceName = "TelesEducacao Auth API",
            Framework = ".NET 9",
            MachineName = Environment.MachineName,
            ProcessorCount = Environment.ProcessorCount,
            WorkingSet = Environment.WorkingSet
        };

        return Ok(result);
    }
}