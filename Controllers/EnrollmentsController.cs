using MarksApi.Dtos;
using MarksApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MarksApi.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPut("enrollments/{id:int}/mark")]
    [Authorize(Roles = "Teacher")]
    public async Task<ActionResult<EnrollmentDetailDto>> UpdateMark(int id, [FromBody] UpdateEnrollmentMarkRequestDto request, CancellationToken ct)
    {
        return Ok(await _enrollmentService.UpdateMarkAsync(id, request, ct));
    }

    [HttpDelete("enrollments/{id:int}")]
    [Authorize(Roles = "Teacher")]
    public async Task<IActionResult> DeleteEnrollment(int id, CancellationToken ct)
    {
        await _enrollmentService.DeleteEnrollmentAsync(id, ct);
        return NoContent();
    }
}
