using MarksApi.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarksApi.Controllers;

[ApiController]
[Route("")]
public class HealthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(AppDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet("health")]
    public async Task<IActionResult> GetHealth(CancellationToken ct)
    {
        try
        {
            var healthy = await _context.Database.CanConnectAsync(ct);
            if (!healthy)
            {
                return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
                {
                    Title = "Service unavailable",
                    Status = StatusCodes.Status503ServiceUnavailable,
                    Detail = "Database connection failed."
                });
            }

            return Ok(new { status = "Healthy" });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Database health check failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Title = "Service unavailable",
                Status = StatusCodes.Status503ServiceUnavailable,
                Detail = "The database is unavailable."
            });
        }
    }
}
